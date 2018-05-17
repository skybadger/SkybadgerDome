using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Ports;
using System.Timers;

using ASCOM;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using ASCOM.Skybadger.Properties;
using System.Globalization;
using System.Collections;

namespace global.Skybadger
{
    public class DeviceComms
    {
        //This class acts as the superclass for the device access class family. 
        //Each form of access device (i2c, RS232, rs485, SPI etc should implement a derivative of this class
        //
        protected Object localLock = new Object();
        public ASCOM.Utilities.TraceLogger logger;
        protected int writeLength;
        protected int readLength;
        protected int targetAddr;
        protected char[] databuff;
        protected ArrayList subDevices;
        protected int numSubDevices = 0;
        protected int id;
        protected System.Collections.Hashtable deviceMap;

        //
        //These functions need implementing in the derived class. 
        //
        public enum ConnectionState { connected, disconnected, partial, reset };
        private abstract ConnectionState Connect( ConnectionState state);// throw NotConnectedException;
        public abstract ConnectionState checkConnected( ConnectionState state);// throw NotConnectedException;
        public abstract int Read(byte proxyAddress, byte address, byte command, out byte[] data);
        public abstract int Write(byte proxyAddress, byte address, byte[] data, int dataLength);
    
        public DeviceComms( Settings config, int deviceId )
        {
          logger.LogMessage( Convert.ToString(deviceId), "Creating device interface");
          //Get the config
          //read the flat set and parse for devices accessed through this hardware interface
          //store into subdevices by address as id
          id = deviceId;
          databuff = new char[256];
          deviceMap  = new Hashtable();
        }
        public void addSubDevice( int deviceId, String deviceName, int deviceAddress )
        {
            if (deviceId >=0  && deviceId <= 255 )
                deviceMap.Add( deviceName, deviceId );
        }

        private void Dispose()
        {
            logger.LogMessage( Convert.ToString(id), "closing connection and cleaning up"); 
        }
    }

    //This class implements access via the serial ports to the I2C interfaces hosted on the remote controllers.
    //There are two different i2c buses - local and dome so there needs to be two instances of this class at the execution level. 
    //Access to read and write is mutexed to prevent re-entrancy through multiple clients talking at the same time.
    public class I2CSerialComms : DeviceComms
     {
        public static const int I2C_RD=1;
        public static const int I2C_WR=0;
        private Object localLock = new Object();
        private ASCOM.Utilities.Serial commPort = new Serial();
        private uint proxyAddress;

        #region public properties and methods

        //constructor with a pre-prepared serial port
        public I2CSerialComms( Settings config, int id) : base ( config, id )
        {
            logger.LogMessage( "debug", "I2CSerialComms constructor called");
            if (id != 0)
            {
                commPort.Port = id;
                Connect( DomeImpl.ConnectionState.CONNECTED);
            }
            else 
                throw new ASCOM.NotConnectedException("Unable to identify device ID or create port at port " + id);
            proxyAddress = 0;
        }

        private void Dispose()
        {
            Connect(DomeImpl.ConnectionState.NOT_CONNECTED);
            commPort.ClearBuffers();
            commPort.Connected = false;
            commPort.LogMessage("DeviceComms:I2cSerialComms.Dispose", "comm port" + commPort.Port + " disposed in Dome driver");

            //super.Dispose();
        }

        public void setProxy( uint newVal )
        {
            if ( newVal >= 0 && newVal <= 255 )
                this.proxyAddress = newVal;
        }
        
        /*
            just a reminder
            private int writeLength;
            private int readLength;
            private int targetAddr;
            private char[] databuff;
            private int[] subDevices;
            private int numSubdevices = 0;
            private int id;
        */
        private ConnectionState Connect( DomeImpl.ConnectionState conn )
        {         
            ConnectionState output = ConnectionState.reset;
            //Need to add exception handling around commPOrt commands
            if ( conn == DomeImpl.ConnectionState.CONNECTED )
            {
                if ( this.commPort.Connected == true )
                {//Already connected
                    logger.LogMessage("TRACE","i2c comm port" + commPort.Port + " already connected in Dome Driver");
                }
                else
                {
                    commPort.Connected = true;
                    commPort.ClearBuffers();
                    //standard comm settings
                    commPort.Handshake = SerialHandshake.None;
                    commPort.Parity = SerialParity.Odd;
                    commPort.StopBits = SerialStopBits.One;
                    commPort.DataBits = 8;
                    commPort.Speed = SerialSpeed.ps19200;
                    commPort.ReceiveTimeoutMs = 250; //throws exception Ascom.InvalidCastException
                    commPort.Connected = true;
                    commPort.LogMessage ( "DeviceComms:I2cSerialComms", " comm port " + commPort.Port + " opened for Dome driver");
                }
                output = ConnectionState.connected;
            }
            else if ( conn == DomeImpl.ConnectionState.NOT_CONNECTED )
            {
                if (commPort.Connected)
                {
                    commPort.ClearBuffers();
                    commPort.Connected = false;
                    commPort.LogMessage ( "DeviceComms:I2cSerialComms", " comm port " + commPort.Port + " closed for Dome driver");
                }
                else
                {
                    //Nothing to do 
                    ;
                }
                output = ConnectionState.disconnected;
            }
            else
            {
                ;//do nothing
            }
            return output;
        }

        public ConnectionState checkConnected( ConnectionState state) // throw DeviceNotConnectedException;
        {
            int i;
            byte[] testData;
            int devicePresent = 0;
            byte[] devicePresentMap = new byte[deviceMap.Count];
            int devicePresentCount = 0; 
            
            for ( i=0; i< deviceMap.Count; i++)
            {
               //Build the I2C query string for devices queried through the remote relay I2C wireless.            
               devicePresent = Read( 0, 
                                (byte) 0x55, 
                                (byte) deviceMap[i], 
                                out testData, 
                                (byte) 1 );
                if (devicePresent > 0 ) 
                {
                    devicePresentMap[i] = 1;
                    logger.LogMessage("TRACE","DeviceComms:i2cCommPort.IsConnected using port " + commPort.Port + " detected device at address " + subDevices[i] + " in Dome Driver");
                    //throw new NotConnectedException( "Failed to find subdevice "+ deviceMap[i] + "required for Skybadger DomeDriver"); 
                }
                else
                {
                    logger.LogMessage("TRACE","DeviceComms:i2cCommPort.IsConnected using port " + commPort.Port + " FAILED  detect device at address " + subDevices[i] + " in Dome Driver");
                    devicePresentMap[i] = 0;
                }
            }
            
            if( devicePresentCount == deviceMap.Count )
                return ConnectionState.connected;
            else if ( ( devicePresentCount < deviceMap.Count ) && ( devicePresentCount > 0 ) )
                return ConnectionState.partial;
            else
                return ConnectionState.disconnected;
        }

        #endregion
        
        #region private methods
               
        //Basic write function for virtual interface. Override based on attached hardware type or not. 
        public int Write( byte proxyAddress, byte targetAddress, byte[] data )
        {
            byte[] outData;
            byte[] output;
            int buffLength = 0;
            int i=0, j=0, writtenCount=0;

            //grab  mutex or wait
            lock(localLock)
            {
                //setup output buffers
                if( proxyAddress == null )
                {
                    buffLength = data.Length+1;
                    outData = new byte[ buffLength];
                }
                else
                {
                    buffLength = data.Length+2;
                    outData = new byte[ buffLength ];
                    outData[i++] = proxyAddress;
                }
                outData[i++] = (byte)( ( targetAddress & 0x7F) | (byte)I2C_WR );
                for ( j=0; j<= buffLength; j++) 
                    outData[i+j] = data[i-1];
                try
                {
                    commPort.TransmitBinary(outData);
                    commPort.ReceiveTimeoutMs = 500;
                    output = commPort.ReceiveCountedBinary( 1 ); //expect status byte
                    writtenCount = output.Length;
                }
                catch (ASCOM.NotConnectedException ex1)
                {
                    commPort.LogMessage("ERROR", "commport not connected error in DeviceComms.I2CSerialComms.i2CWrite");
                }
                catch (ASCOM.Utilities.Exceptions.SerialPortInUseException ex2)
                { 
                    commPort.LogMessage("ERROR", "commport already in use error in DeviceComms.I2CSerialComms.i2CWrite");
                }                
            }
            
        return writtenCount;
        }

        //Basic read function for virtual interface. Override based on attached hardware type or not. 
        public int Read( byte proxyAddress, 
                            byte address, 
                            byte command, 
                            out byte[] outData, 
                            byte length)
        {
            int i = 0;
            int receivedByteCount = 0;
            byte[] localOutData;
            byte[] localInData;
            byte inByte;

            //grab  mutex or wait
            lock (localLock)
            {
                i=0;
                //first byte is proxy command or not required. 
                if( proxyAddress == null)
                    localOutData = new byte[2];
                else
                {
                    localOutData = new byte[3];
                    localOutData[i++] = proxyAddress;
                }

                localInData = new byte[length];
                localOutData[++i] = (byte) ( (address & 0xFE ) |  I2C_RD );
                localOutData[i] |= (byte) I2C_RD;
                localOutData[++i] = command;

                try
                {
                    commPort.TransmitBinary ( localOutData );
                    commPort.ClearBuffers();
                    commPort.ReceiveTimeoutMs = 500;
                    localOutData = commPort.ReceiveCountedBinary( length );
                }
                catch (ASCOM.NotConnectedException ex1)
                { 
                }
                catch (ASCOM.Utilities.Exceptions.SerialPortInUseException ex2)
                {
                }

            }            
       outData = localInData;
       return localInData.Length;
       }
    
        #endregion
    }        
    
    public class CmdTuple
    {
        Object tuple_1, tuple_2;
        public CmdTuple( Object tuple_1, Object tuple_2)
        {
            this.tuple_1 = tuple_1;
            this.tuple_2 = tuple_2;
        }
        
        public global.Skybadger.DomeImpl.DomeCmds getTuple_1() 
        {
            if( this.tuple_1.GetType() == typeof(Skybadger.DomeImpl.DomeCmds) )    
                return (Skybadger.DomeImpl.DomeCmds) this.tuple_1;
            else
                throw new System.FieldAccessException("Failed to convert object to DomeCmds enum in tuple_1");
        }
        
        public double getTuple_2( )
        {
            if( this.tuple_2.GetType() == typeof( double) )            
                return (double) this.tuple_2;
            else
                throw new System.FieldAccessException("Failed to convert object to double in tuple_2" );
        }

        public void Dispose ()
        {
            this.tuple_1 = null;
            this.tuple_2 = null;
        }
    }

    public class DomeImpl
    {
        public ASCOM.Utilities.TraceLogger logger;

        public enum ConnectionState { NOT_CONNECTED, PARTIAL_CONNECT, CONNECTED, RESET };
        public enum DomeStates { PARKED, HOMED, STOPPED, SHUTTER_OPENING, SHUTTER_CLOSING, CONFUSED, SLEW_CW, SLEW_CCW, ABORTING, SLAVED };
        //These are commands that need to be operated in order since we may have multiple clients sending commands. 
        public enum DomeCmds { ABORT, PARK, SLEW_TO_AZIMUTH, SLEW_TO_ALTITUDE, SYNC_TO_AZIMUTH, SET_SLAVED, OPEN_SHUTTER, CLOSE_SHUTTER }; 
        public static double positioningPrecision = 5.0;

        //State info
        private System.Collections.Queue cmdQueue = new Queue(10);
        public int altitude, altitudeTarget;
        public int homePosition, parkAzimuth;
        public float azimuth, azimuthTarget;
        public int slotAltitudeMin, slotAltitudeMax;
        //Dome switches - top and bottom of slot.
        bool domeOpenSwitch = false;
        bool domeClosedSwitch = true;

        public DomeStates domeState = DomeStates.PARKED;
#region private
        private DateTime julianDateSeconds;

        private float magnetometer; 
        private float battery;

        private ConnectionState eConnected;
        
        private StringBuilder outString;
        private byte[] outByte = new byte[65];

        //Hardware interfaces
        DeviceComms  domePort;
        DeviceComms obboPort;
        private ASCOM.Skybadger.Properties.Settings config;
      
        int numPorts = 0;
#endregion
 
      //Needs a value map object
      //Needs a delegate to call events.  
      private static System.Timers.Timer aTimer;
      private static void OnTimedEvent(Object source, ElapsedEventArgs e)
      {
          byte[] outData = new byte[64];
          byte command = 0;
          byte address = 0;
          byte length;
          
          //logger.LogMessage(this.GetType().ToString(), "Dome class timer tick");
          //Query all connected devices for status update
          //Read azimuth
          //domePort.read( domePort.address("MAGNETOMETER"), 0, outData, length );

          //Check rain sensor
          //Update clock
         //Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);

      }

#region public     
        public void Dispose()
        {
            logger.LogMessage(this.GetType().ToString(), "Dome class disposing");
            if ( domePort.checkConnected(DeviceComms.ConnectionState.disconnected) != DeviceComms.ConnectionState.disconnected)

            //write back state we want to keep 
            config.Save();
            //turn off timer
            aTimer.Enabled = false;
        }

        DomeImpl(ASCOM.Skybadger.Properties.Settings config)
        {
         this.config = config; 
         domePort = new I2CSerialComms( config, (int)  config.DomeCommPort );
         obboPort = new I2CSerialComms( config,  (int) config.ObboCommPort );
         numPorts = 2;

         logger = new TraceLogger();

         // Create a timer with a two second interval.
         aTimer = new System.Timers.Timer(1000);
         // Hook up the Elapsed event for the timer. 
         aTimer.Elapsed += OnTimedEvent;
         aTimer.Enabled = true;

         azimuth = config.SlitAzimuth;
         altitude = config.SlitAltitude;
         parkAzimuth = config.ParkPosition;
         julianDateSeconds = System.DateTime.Now;
         azimuthTarget = azimuth;
         altitudeTarget = altitude;
         slotAltitudeMin = 0;
         slotAltitudeMax = 100;
         bool bConnected = false;
         ConnectionState eConnected = ConnectionState.NOT_CONNECTED;
       }
      
      public ConnectionState IsConnected
      {
         get {  return eConnected; }
         set {                     
                  try
                  {
                     switch (value)
                     {
                     case ConnectionState.CONNECTED:
                        logger.LogMessage(this.GetType().ToString(), "setting 'connected' when already connected");
                        break;
                     case ConnectionState.NOT_CONNECTED:
                        logger.LogMessage(this.GetType().ToString(), "setting connected when not connected");
                        break;
                     }
                  } 
                  catch ( ASCOM.NotConnectedException dnce )
                  {
                     value = ConnectionState.NOT_CONNECTED;
                     logger.LogMessage( this.GetType().ToString(), "Did not Connect Exception setting new connection state. Current is: " + eConnected + " and desired is:" + value );
                  }
            }
      }
        

      private bool rotateDome( int target )
      {
        bool status = false;
        bool direction = true;
        byte[] outData = new byte[5];
        String dateString; 
        String stateString;
        char[] charString;
        int magnitude = (target - (int) azimuth)/360;

        //Check  current azimuth
        //Estimate shortest direction and distance to travel 
          if( magnitude > 0 && magnitude <180 )
          {
              direction = true;
          }
          else if ( magnitude > 180 && magnitude <360 ) 
          {
              direction = false; 
              magnitude = magnitude -180;
          }
          else if( magnitude < 0 && magnitude > -180 )
          {
              direction = false;               
          }
          else if( magnitude < -180 && magnitude > -360 )
          {
              direction = true;               
              magnitude = 360 + magnitude;
          }              

        //Setup slew
        //LCD                         
        //Move to row 4
        outData[0] = (byte) 0x01 ;//go to row 
        outData[1] = (byte) 0x03 ;//row        
        if ( obboPort.Write( (byte) 0x55, (byte) config.I2CLCDAddr, outData, 2 ) )
            retVal = 0;
        //Move to first col
        outData[0] = (byte) 0x02 ;//go to col 
        outData[1] = (byte) 0x00 ;//col
        if ( obboPort.Write( (byte) 0x55, (byte) config.I2CLCDAddr, outData, outData.Length ) )
            retVal = 0;
        //Write 'slewing' string 
        stateString = "slewing";
        outData = new byte[stateString.Length];
        charString = stateString.ToCharArray();
        for( int i = 0; i< outString.Length; i++)
        {
            outData[i] = (byte) charString[i];
        }
        if ( obboPort.Write( (byte) 0x55, (byte) config.I2CLCDAddr, outData, outData.Length ) )
            retVal = 0;

        //Magnetometer - update azimuth
        outString  = domePort.Read( 0x55, config.I2CMagnetometerAddress, outData, outData.Length );
        
        //motor controller
        //Set motor direction
        outData[0] = (byte) 0x01 ;//set motor direction
        outData[1] = (byte) 0x03 ;//motor direction        
        if ( obboPort.Write( (byte) 0x55, (byte) config.I2CLCDAddr, outData, outData.Length ) )
            retVal = 0;

        //Wait for complete
        while( (target - azimuth) > (int) positioningPrecision )
        {
            if ()
            {
            }
            //Set motor speed
            outData[0] = (byte) 0x01 ;//set motor direction
            outData[1] = (byte) 0x03 ;//motor direction        
            if ( obboPort.Write( (byte) 0x55, (byte) config.I2CLCDAddr, outData, 2 ) )
                retVal = 0;

            //Magnetometer - update azimuth
            outString  = domePort.Read( 0x55, config.I2CMagnetometerAddress, outData, 2 );
            
            //delay here if poss - maybe a 'sleep(500)' 
        }  
          
          //Check we have landed.
        if(( target - azimuth ) < (int) positioningPrecision )
        {
            domeState = DomeStates.STOPPED;   
            status = true;
        }
        else 
        {
            logger.LogMessage(this.GetType().ToString(), "Failed to reach requested dome azimuth: " + target );

        }

        return status;
      }

      private bool moveShutter( int target )
      {
        bool status = false;
        //Check current state
        //Setup slew
        //wait for complete
        //if closed - chec llocks and travel limit switches
        //if open check travel limit switch 
        //if in-between, check position sensors. 
        return status;
      }

      //Handles synchronous commands that need queueing for long running operations. 
      public bool syncCommand( DomeCmds inCmd, double cmdArg )
      {
         bool outputStatus = false;
         if ( inCmd == DomeCmds.ABORT )
         {
             //clear down the existing queue - leave state at aborting. 
             domeState = DomeStates.ABORTING;
             this.cmdQueue.Clear();
             logger.LogMessage( this.GetType().ToString(), " ABORT coomand detected - clearing CMD queue of: " + cmdQueue.Count + " entries");
             //let timer handle the dome and shutter. 
         }
         else
         {
             //NOT IMPLEMENTED: enqueue the new command in case there are existing long-running commands. 
             // this allows the driver to respond rapidly but long commands to take their time in the right order for multiple clients. 
             switch( inCmd )
             {
             case DomeCmds.PARK:
                     outputStatus = rotateDome( parkAzimuth);
                     break;
             case DomeCmds.SLEW_TO_AZIMUTH: 
                     outputStatus = rotateDome( (int) cmdArg );
                     break;
             case DomeCmds.SLEW_TO_ALTITUDE:
                     outputStatus = moveShutter( (int) cmdArg );
                     break;
             case DomeCmds.SYNC_TO_AZIMUTH:
                     azimuth = (int)cmdArg;
                     outputStatus = true;
                     break;
             case DomeCmds.SET_SLAVED:
                     azimuth = (int)cmdArg;
                     outputStatus = true;
                     break;
             case DomeCmds.OPEN_SHUTTER:
                     outputStatus = moveShutter( 110 );
                     break;
             case DomeCmds.CLOSE_SHUTTER:
                     outputStatus = moveShutter( 0 );
                     break;
//                 logger.LogMessage( this.GetType().ToString(), " Setting new CMD tuple: " + inCmd + " with value: " + cmdArg + " to queue");
//                 cmdQueue.Enqueue( new CmdTuple( inCmd, cmdArg ) );
             default: 
                 throw new ASCOM.InvalidOperationException("This cmd is not implemented in syncCommand handler, check for direct function calls.");
                 break;
             }       
         }
         return true;
      }

      public void Dispose()
      {
         //write config back to setting where appropriate.
          config.Save();
      }

#endregion

#region private
      private ConnectionState Connected( ) 
      {
          String tString = "Time:";
          String Welcome = "Welcome to skybadger Dome";
          int retVal = 0;
          byte[] outData; 
          byte[] inData;
          int inDataLength = 0;
          String outString;
          char[] outChars;

          try
         {
               //Connect to Dome controller
               //Check it's responding then check sub devices.
               //battery monitor 
               outData = new byte[tString.Length + System.DateTime.Now.ToLongTimeString().Length ];
               outString  = tString + System.DateTime.Now.ToLongTimeString();
               outChars = outString.ToCharArray();
               for( int i = 0; i< outString.Length; i++)
               {
                    outData[i] = (byte) outChars[i];
               }

               inDataLength = obboPort.Read( (byte)( 0x55), 
                                        (byte)(config.I2CMagnetometerAddr), 
                                        0, 
                                        out inData, 
                                        2 ); 

              //Magnetometer
               outData = new byte[tString.Length + System.DateTime.Now.ToLongTimeString().Length ];
               outString  = tString + System.DateTime.Now.ToLongTimeString();
               outChars = outString.ToCharArray();
               for( int i = 0; i< outString.Length; i++)
               {
                    outData[i] = (byte) outChars[i];
               }

              // 
               //Connect to obbo Controller
               //Check it's responding then check sub devices. 
               //motor controller

               //LCD                         
               outData = new byte[tString.Length + System.DateTime.Now.ToLongTimeString().Length ];
               outString  = tString + System.DateTime.Now.ToLongTimeString();
               outChars = outString.ToCharArray();
               for( int i = 0; i< outString.Length; i++)
               {
                    outData[i] = (byte) outChars[i];
               }
              //Temp sensor
              //Humidity sensor
              //Battery voltage

               if ( obboPort.Write( (byte) 0x55, (byte) config.I2CLCDAddr, outData, outData.Length ) )
                   retVal = 0;
        }
        catch(SystemException seEx )
        {

        }
        finally
        {
           if( retVal == 0 )
                this.eConnected = ConnectionState.CONNECTED;
           else
               this.eConnected = ConnectionState.NOT_CONNECTED;

        }
       return this.eConnected;
      }
#endregion
//end of namespace
}