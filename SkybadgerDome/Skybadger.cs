#define HMC5883_MAGNETOMETER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using ASCOM;
using ASCOM.DeviceInterface;
using ASCOM.Skybadger.Properties;
using ASCOM.Utilities;
using System.Threading;

namespace ASCOM.Skybadger
{
    public class DomeImpl
    {
        public static byte HIGH_SLEW_SPEED = 200;
        public static byte LOW_SLEW_SPEED = 90;
        public static byte LOW_SPEED_DISTANCE = 10;

        public static ASCOM.Utilities.TraceLogger logger;
        public enum portType 
        { 
            DOME_PORT, 
            OBBO_PORT 
        };
        public enum ConnectionState
        {
            NOT_CONNECTED,
            PARTIAL_CONNECT,
            CONNECTED,
            RESET
        };
        public enum DomeStates
        {
            PARKED,      //implies STOPPED
            HOMED,      //IMPLIES STOPPED
            DOME_STOPPED, 
            SLEW_CW, SLEW_CCW,
            SLEW_CW_TIMED, SLEW_CCW_TIMED, 
            SLAVED,
            CONFUSED
        };

        public String[] DomeStateStrings = 
        {
        "PARKED", "HOMED", "STOPPED", 
        "SLEW CW", "SLEW CCW", "SLEW CW TIMED", "SLEW CCW TIMED", "SLAVED", "CONFUSED" 
        };

        public enum ShutterStates
        {
            SHUTTER_OPENING, SHUTTER_CLOSING, SHUTTER_STOPPED, SHUTTER_OPEN, SHUTTER_CLOSED,
            CONFUSED
        };

        public String[] ShutterStateStrings = 
        {
        "OPENING", "CLOSING", "STOPPED", 
        "OPENED", "CLOSED", "CONFUSED"
        };
        
        //These are commands that need to be operated in order since 
        //we may have multiple clients sending commands. 
        public enum DomeCmds
        {
            ABORT,
            PARK,
            SLEW_TO_AZIMUTH,  //under control of bearing supplied.
            SLEW_TO_ALTITUDE,
            SYNC_TO_AZIMUTH,
            OPEN_SHUTTER,
            CLOSE_SHUTTER,
            SET_SLAVED,
            ROTATE_CW_TIMED, //not under control of bearing
            ROTATE_CCW_TIMED,//not under control of bearing
            CUSTOM
        };
        public static float positioningPrecision = 2.5F;

        //State info
        public int altitude, altitudeTarget;
        public int homePosition, parkAzimuth;
        public float azimuth, azimuthTarget, azimuthSyncOffset;
        
        public int slotAltitudeMin, slotAltitudeMax;
        public DomeStates domeState = DomeStates.PARKED;
        public ShutterStates shutterState = ShutterStates.SHUTTER_STOPPED;

        //Dome switches - top and bottom of slot.
        bool shutterOpenedSwitch = false;
        bool shutterClosedSwitch = true;
        bool shutterLockSwitch = true;
        
        private DateTime julianDateSeconds;
        //this is used not just for timed slews but for timing the bearing-based slews to detect failure conditions. 
        private DateTime cmdTimeout = new System.DateTime();
        private static int tickCount = 0;
        private float voltage = 0F; 
        private ConnectionState eConnected = ConnectionState.NOT_CONNECTED;
        private byte[] outByte = new byte[65];
        private System.Collections.Queue cmdQueue = new Queue(10);

        //Hardware interfaces
        DeviceComms domePort;
        DeviceComms obboPort;
        
        int numPorts = 0;

        public DomeImpl()
        {
            logger = new TraceLogger("", "Dome");
            logger.Enabled = true;
            Console.WriteLine( " log file logged to: " +  logger.LogFileName);

            // Create a timer with a second interval.
            aTimer = new System.Timers.Timer(1000);
            aTimer.AutoReset = true;

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            
            //Enable the timer after we have connected
            //Only once connected - hence in IsConnected/disconnected
            
            domeState = DomeStates.DOME_STOPPED;
            shutterState = ShutterStates.SHUTTER_STOPPED;

            azimuth = ASCOM.Skybadger.Properties.Settings.Default.SlitAzimuth;
            azimuthTarget = azimuth;

            altitude = ASCOM.Skybadger.Properties.Settings.Default.SlitAltitude;
            altitudeTarget = altitude;
            slotAltitudeMin = 0;
            slotAltitudeMax = 100;

            parkAzimuth = ASCOM.Skybadger.Properties.Settings.Default.ParkPosition;
            julianDateSeconds = System.DateTime.Now;

            eConnected = ConnectionState.NOT_CONNECTED;
        }

        //Needs a value map object

        //Needs a delegate to call events.  
        private static System.Timers.Timer aTimer;
        private void OnTimedEvent( Object source, ElapsedEventArgs e)
        {
            byte[] outData = new byte[64];
            
            tickCount++;
            logger.LogMessage(this.GetType().ToString(), "OnTimedEvent::Dome class timer tick entered");
            //Query all connected devices for status update

            if (domePort == null || obboPort == null)
            {
                //Timer has been stopped as part of disconnect/shutdown but this has still gone off
                logger.LogMessage(this.GetType().ToString(), "OnTimedEvent::Timer ticked but no valid ports open.");
                return;
            }
            
            //update azimuth and dependent async funcs every second
            //Every tick
            this.azimuth = domePort.GetBearing();
            logger.LogMessage(this.GetType().ToString(), "OnTimedEvent::Current raw bearing:" + this.azimuth);

            //check for completion of dome slew managed by compass bearing
            //assumes we don't overshoot outside the required pointing precision in the space of one timer tick. 
            //assume bearing is reliably measured to suitable accuracy.
            if ( checkRotateDomeAsyncComplete() )
                logger.LogMessage(this.GetType().ToString(), "OnTimedEvent::Check for end async rotate dome detected");

            //check for completion of shutter close/open
            if ( checkMoveShutterAsyncComplete() )
                logger.LogMessage(this.GetType().ToString(), "OnTimedEvent::Check for end Close async shutter move detected");
            
            if ( checkRotateDomeTimedComplete() )
                logger.LogMessage(this.GetType().ToString(), "OnTimedEvent:Check for end Timed rotate dome detected");
            
            //Check for background long running tasks. Take one off the stack and execute.
            if (this.domeState != DomeStates.DOME_STOPPED)
                logger.LogMessage(this.GetType().ToString(), "OnTimedEvent::Cmd running: " + this.DomeStateStrings[(int)domeState]);
            else if (!executeAsyncCmd())
                logger.LogMessage(this.GetType().ToString(), "OnTimedEvent::No async cmd to execute ");
            
            //Every 5 timeouts do some housekeeping
            if (tickCount % 5 == 0 )
            {
                logger.LogMessage(this.GetType().ToString(), "OnTimedEvent::low frequency checks");
                tickCount = tickCount % 10;
                //Check rain sensor
                /*
                 * if( this.isRaining = true )
                 * {
                 * //Send a MMQT message and then close the dome
                 * //Abort slews & empty queue
                 * syncCommand( DomeImpl.DomeCmds.ABORT_CMD );
                 * //Close shutter
                 * syncCommand( DomeImpl.DomeCmds.SHUTTER_CLOSE_CMD);
                 * //Slew to park
                 * syncCommand( DomeImpl.DomeCmds.PARK_CMD);
                 * }
                 * 
                */
            }
                        
            //Update clock on display
            //obboPort.WriteLCD(2, 0, System.DateTime.UtcNow.ToShortTimeString() );

            //Update dome status on display
            //obboPort.WriteLCD(3, 0, "Dome state: " + DomeStateStrings[ (int) domeState ] );           
        }

        #region public
        public void Dispose()
        {

            DomeImpl.logger.LogMessage(this.GetType().ToString(), "DomeImpl:Dispose:: Dome class disposing");

            //turn off timer
            aTimer.Enabled = false;

            //Disconnect devices
            domePort.Connect( ConnectionState.NOT_CONNECTED);
            obboPort.Connect( ConnectionState.NOT_CONNECTED);

            //write back state we want to keep 
            ASCOM.Skybadger.Properties.Settings.Default.Save();     
        }
        
        /*
         dome rotate using bearing to determine rate and direction*/
        public bool rotateDomeAsyncStart(int target)
        {
            bool status = false;
            bool direction = true;
            byte speed = 0;
            byte[] outData = new byte[5];
            int magnitude;
           
            this.azimuthTarget = target;
            magnitude = (int)( target - (int)(this.azimuth + this.azimuthSyncOffset)) % 360;

            //Check  current azimuth
            //Estimate shortest direction and distance to travel 
            if (magnitude > 0 && magnitude < 180)
            {
                direction = true;
            }
            else if (magnitude > 180 && magnitude < 360)
            {
                direction = false;
                magnitude = magnitude - 180;
            }
            else if (magnitude > -180 && magnitude < 0 )
            {
                direction = false;
            }
            else if (magnitude > -360 && magnitude < -180 )
            {
                direction = true;
                magnitude = (360 - magnitude  ) % 360;
            }

            //motor controller
            if ( Math.Abs(magnitude) < LOW_SPEED_DISTANCE )
                speed = (byte) LOW_SLEW_SPEED;
            else
                speed = (byte) HIGH_SLEW_SPEED; 

            try
            {
                //Start slew
                status  = obboPort.SetSpeedDirection( speed, direction );
                if (status)
                {
                    if (direction == true)
                        domeState = DomeStates.SLEW_CW;
                    else
                        domeState = DomeStates.SLEW_CCW;

                    //set cmd timeout timer in case of failure to move. 
                    cmdTimeout = System.DateTime.Now.AddMinutes(5);

                    logger.LogMessage(this.GetType().ToString(), "rotateDomeAsyncStart:: started dome slew to azimuth: " +
                            this.azimuthTarget + " from :" +
                            this.azimuth + this.azimuthSyncOffset );
                }

            }
            //catch ANY errors and try to flag and fail safe
            catch (System.Exception ex)
            {
                logger.LogMessage(this.GetType().ToString(), "rotateDomeAsyncStart::Collective failure to write to remote device: " + ex.Message);
                
                //Set motor direction
                obboPort.SetSpeedDirection( speed = 0, direction );
                domeState = DomeStates.DOME_STOPPED;

                status = false;
            }
            return status;
        }

        public Boolean checkRotateDomeAsyncComplete()
        {
            float localAzimuth, localTarget;
            byte[] returnData = new byte[2];
            byte[] outData = new byte[2];
            float signedOverrun; 
            float absOverrun;
            Boolean status = false;

            logger.LogMessage(this.GetType().ToString(), "checkRotateDomeAsyncComplete::Entered ");                    
            //grab a copy of the azimuth reading in case it changes. 
            localAzimuth = azimuth;
            localTarget = this.azimuthTarget;
            signedOverrun = localTarget - localAzimuth;
            absOverrun = System.Math.Abs(signedOverrun); 
            
            //Looking for dome slew complete, expect to see dome currently slewing. 
            if (domeState != DomeStates.SLEW_CCW && domeState != DomeStates.SLEW_CW )
            {
                logger.LogMessage(this.GetType().ToString(), "checkRotateDomeAsyncComplete:: Dome state says not slewing");
                return false;
            }

            try
            {
                //confirm whether we have landed
                if (absOverrun < positioningPrecision || DateTime.Compare(cmdTimeout, System.DateTime.Now) < 0)
                {
                    //Set motor direction
                    logger.LogMessage(this.GetType().ToString(), "checkRotateDomeAsyncComplete::Target position matched - halting slew");
                    if (obboPort.SetSpeedDirection(0, true))
                    {
                        domeState = DomeStates.DOME_STOPPED;
                        status = true;
                    }
                }
                //Confirm whether we are close and so slow down.
                else if (absOverrun < LOW_SPEED_DISTANCE )
                {
                    //Set motor direction
                    logger.LogMessage(this.GetType().ToString(), "checkRotateDomeAsyncComplete::Target position close - slowing slew");
                    if ( domeState == DomeStates.SLEW_CW )
                        status = obboPort.SetSpeedDirection( LOW_SLEW_SPEED, true);
                    else
                        status = obboPort.SetSpeedDirection( LOW_SLEW_SPEED, false);
                    
                    if ( status == false )
                        logger.LogMessage(this.GetType().ToString(), "checkRotateDomeAsyncComplete:: slowing slew failed");
                }
                else if (domeState == DomeStates.SLEW_CCW && System.Math.Sign(signedOverrun) > 0  )
                {
                    //Error - dome overshoot detected
                    //Set motor direction reversed and low dome speed.
                    logger.LogMessage(this.GetType().ToString(), String.Format( "checkRotateDomeAsyncComplete::Overrun detected - reversing slew, target {0}, current {1}", azimuthTarget, localAzimuth));
                    if (absOverrun > LOW_SPEED_DISTANCE )
                    {
                        if (obboPort.SetSpeedDirection( HIGH_SLEW_SPEED, true))
                            domeState = DomeStates.SLEW_CW;
                    }
                    else
                    {
                        if (obboPort.SetSpeedDirection( LOW_SLEW_SPEED, true))
                            domeState = DomeStates.SLEW_CW;
                    }
                }
                else if (domeState.Equals(DomeStates.SLEW_CW) && System.Math.Sign(signedOverrun) < 0 )
                {
                    //Error - dome overshoot detected
                    //Set motor direction reversed and low dome speed.
                    logger.LogMessage(this.GetType().ToString(), String.Format("checkRotateDomeAsyncComplete::Overrun detected - reversing slew, target {0}, current {1}", azimuthTarget, localAzimuth));
                    if ( absOverrun > LOW_SPEED_DISTANCE )
                    {
                        if (obboPort.SetSpeedDirection( HIGH_SLEW_SPEED, false))
                            domeState = DomeStates.SLEW_CCW;
                    }
                    else
                    {
                        if (obboPort.SetSpeedDirection( LOW_SLEW_SPEED, false))
                            domeState = DomeStates.SLEW_CCW;
                    }
                }
            }
            catch (System.Exception ex )
            {
                logger.LogMessage(this.GetType().ToString(), "checkRotateDomeAsyncComplete::Failure to terminate slew nicely:" + ex.Message);
                if (obboPort.SetSpeedDirection(0, true))
                    domeState = DomeStates.DOME_STOPPED;
                status = true;//unequivocally.
            }
            finally
            {
                ///If the motors are not set on this timer event based handler - they wil be set on the next timer-driven attempt .
                ///Leave the slewing state alone on that basis. 
                //set motor to zero again ?
                logger.LogMessage(this.GetType().ToString(), "checkRotateDomeAsyncComplete::Failure to write to remote device: ");                    
            }
            return status;
        }

        /*
         * 
        */
        private bool moveShutterAsyncStart(int target)
        {
            bool status = false;
            bool direction = true;
            byte speed = 0;
            byte[] outData = new byte[5];
            int magnitude;

            this.altitudeTarget = target;
            magnitude = (target - (int)this.altitude) % 360;
                      
            //Check  current azimuth
            //Estimate shortest direction and distance to travel 
            if (magnitude > 0 && magnitude < 110)
            {
                direction = true;
            }
            else if (magnitude < 0 )
            {
                direction = false;
            }

            try
            {
                //Setup shutter move
                //Write slewing state to LCD                         
                //domePort.WriteLCD(3, 0, message);

                //shutter motor controller
                if (magnitude > LOW_SPEED_DISTANCE )
                    speed = HIGH_SLEW_SPEED;
                else
                    speed = LOW_SLEW_SPEED;

                //domePort.SetSpeedDirection(speed, direction);

                if (direction == true)
                    shutterState = ShutterStates.SHUTTER_OPENING;
                else
                    shutterState = ShutterStates.SHUTTER_CLOSING;

                status = true;
                logger.LogMessage(this.GetType().ToString(), "moveShutterAsyncStart::Successfully started shutter slew to altitude: " +
                        this.altitudeTarget + " from :" +
                        this.altitude);
            }
            //catch ANY errors and try to flag and fail safe
            catch (System.Exception ex)
            {
                logger.LogMessage(this.GetType().ToString(), "moveShutterAsyncStart::Collective failure to write to remote device: " + ex.Message);

                //Set motor direction
                if ( domePort.SetSpeedDirection( 0, direction) )
                    shutterState = ShutterStates.SHUTTER_STOPPED; //dodgy

                //Update LCD message
                //domePort.WriteLCD(3, 0, "Err; Shutter Halted");

                //Get current bearing - leave to timer.
                status = false;
            }

            //Check current state
            //Setup slew
            //if closed - chec locks and travel limit switches
            //if open check travel limit switch 
            //if in-between, check position sensors. 
            logger.LogMessage("moveShutterAsyncStart", "started");
            return status;
        }

        /*
         * Return false for nothing to do
         * Return true for current action completed & tidied up 
         * 
         */
        private bool checkRotateDomeTimedComplete()
        {
            int timeout = 0;
            bool output = false;

            if (domeState != DomeStates.SLEW_CCW_TIMED && domeState != DomeStates.SLEW_CW_TIMED)
                return output= false;

            logger.LogMessage(this.GetType().ToString(), "checkRotateDomeTimedComplete:: Timed slew running");
            timeout = DateTime.Compare(cmdTimeout, System.DateTime.Now);
            if (timeout < 0)
            {
                logger.LogMessage(this.GetType().ToString(), "checkRotateDomeTimedComplete:Timed slew time detected expired");
                //stop slew.
                if (this.obboPort.SetSpeedDirection(0, true))
                {
                    //Update state unless call fails - at which point let timer try again next time
                    domeState = DomeStates.DOME_STOPPED;
                    output = true;
                }
                else
                {
                    logger.LogMessage(this.GetType().ToString(), "checkRotateDomeTimedComplete::Attempt to stop timed slew failed due to SetSpeed returning false");
                }
            }
            return output;
        }

        /*
         */
        private bool checkMoveShutterAsyncComplete()
        {
            bool status = false;

            logger.LogMessage(this.GetType().ToString(), "checkMoveShutterAsyncComplete:: Entered");
            //Check current state
            //if closed - chec locks and travel limit switches
            if (this.shutterState == ShutterStates.SHUTTER_CLOSING && shutterClosedSwitch)
            {                
                //turn off shutter motor 
                //check shutter lock
            }
            //if open check travel limit switch 
            else if (this.shutterState == ShutterStates.SHUTTER_OPENING && shutterOpenedSwitch)
            {
                //turn off shutter motor.

                logger.LogMessage(this.GetType().ToString(), "checkMoveShutterAsyncComplete:: Open async shutter end move detected in timer");
            }
            
            //if in-between, check position sensors. 

            return status;
        }

        private bool executeAsyncCmd()
        {
            bool outputStatus = false;
            CmdTuple cmd;
            DomeCmds inCmd;
            
            Object myMutex = new Object();
            lock ( myMutex)
            {
                if (cmdQueue.Count > 0)
                {
                    cmd = (ASCOM.Skybadger.CmdTuple)this.cmdQueue.Dequeue();
                    // could use cmdQueue.Peek();
                    inCmd = cmd.getTuple_1();
                }
                else
                    return false;

                switch (inCmd)
                {
                    case DomeCmds.PARK:
                        outputStatus = rotateDomeAsyncStart(parkAzimuth);
                        break;
                    case DomeCmds.SLEW_TO_AZIMUTH:
                        outputStatus = rotateDomeAsyncStart((int)cmd.getTuple_2());
                        break;
                    case DomeCmds.SLEW_TO_ALTITUDE:
                        outputStatus = moveShutterAsyncStart((int)cmd.getTuple_2());
                        break;
                    case DomeCmds.SYNC_TO_AZIMUTH:
                        azimuth = (int)cmd.getTuple_2();
                        outputStatus = true;
                        break;
                    case DomeCmds.SET_SLAVED:
                        //azimuth = (int)cmd.getTuple_2();
                        outputStatus = false;
                        break;
                    case DomeCmds.OPEN_SHUTTER:
                        outputStatus = moveShutterAsyncStart(110);
                        break;
                    case DomeCmds.CLOSE_SHUTTER:
                        outputStatus = moveShutterAsyncStart(0);
                        break;
                    default:
                        logger.LogMessage(this.GetType().ToString(), "executeAsyncCmd::This cmd (" + inCmd + ") is not implemented in AsyncCommand handler, check for direct function calls.");
                        outputStatus = false;
                        break;
                }
                logger.LogMessage(this.GetType().ToString(), "executeAsyncCmd:: executeAsyncCmd:: New async command queued for timer execution");
                if ( !outputStatus)
                {
                    //Do we want to re-queue or just fail and move on ? 
                    //De-queue on successful operation and leave alone if not for retry on next timer interval ?
                }
            }
        return outputStatus;
        }

        //Handles synchronous commands that aren't directly related to dome operation 
        public String syncCommand( String input )
        {
            String output = "";

            //handle 'stateless' commands 
            if ( String.Compare(input, "Bearing") != 0 && String.Compare(input, "Voltage") != 0 && 
                String.Compare(input, "rotateLeft") != 0 && String.Compare(input, "rotateRight") != 0 )
            {
                logger.LogMessage(this.GetType().ToString(), "syncCommand::Unable to parse argument in CommandString. Supported commands are: 'rotateLeft', 'rotateRight', 'Bearing', 'Voltage'");
                output = "Unable to parse argument in CommandString. Supported commands are: 'rotateLeft', 'rotateRight', 'Bearing', 'Voltage'";
                return output;
            }

            if (String.Compare(input, "Bearing") == 0 || String.Compare(input, "Voltage") == 0 )
            {
                if ( String.Compare(input, "Bearing") == 0 ) 
                    output = String.Format("{0}", this.azimuth + this.azimuthSyncOffset);
                else if ( String.Compare(input, "Voltage") == 0 )
                    output = String.Format ("{0}", this.voltage );
                return output;
            }

            //Remaining actions are command actions 
            if (this.domeState != DomeStates.DOME_STOPPED)
            {
                output = String.Format("syncCommand::Dome not yet stopped - use isSlewing before this command. ");
                return output;
            }

            if (this.cmdQueue.Count > 0)
                return output = "1 or more async commands are currently pending - try again later";
            
            if ( String.Compare(input, "rotateLeft") == 0 )
            {
                if ( this.obboPort.SetSpeedDirection( LOW_SLEW_SPEED, true) )
                {
                    this.domeState = DomeStates.SLEW_CW_TIMED;
                    this.cmdTimeout = System.DateTime.Now.AddSeconds(30);
                    output = String.Format("Rotation begun - 30 seconds");
                }
                else
                {
                    output = String.Format("Dome move command failed");
                }
            }

            else if (String.Compare(input, "rotateRight") == 0 ) 
            {
                if ( this.obboPort.SetSpeedDirection( LOW_SLEW_SPEED, false)  )
                {
                    this.domeState = DomeStates.SLEW_CCW_TIMED;
                    this.cmdTimeout = System.DateTime.Now.AddSeconds(30);
                    output = String.Format("Rotation begun - 30 seconds");
                }
                else
                {
                    output = String.Format("Dome move command failed");
                }
            }
            return output;
        }

        public bool syncCommand(DomeCmds inCmd, double cmdArg)
        {
            bool output = false;
            Object mutex = new Object();
            lock (mutex)
            {
                if (inCmd == DomeCmds.ABORT)
                {
                    //clear down the existing queue - leave state at aborting. 
                    logger.LogMessage(this.GetType().ToString(), "syncCommand:: ABORT command detected - clearing CMD queue of: " + cmdQueue.Count + " entries");
                    this.cmdQueue.Clear();

                    //Let the the timer handler handle this.
                    if (domeState != DomeStates.DOME_STOPPED)
                        output = domePort.SetSpeedDirection( 0, false);
                    
                    //Set the timer for timeout of operations. 
                    this.cmdTimeout = DateTime.Now;
                }
                else
                {
                    logger.LogMessage(this.GetType().ToString(), "syncCommand::Setting new CMD tuple: " + inCmd + " with value: " + cmdArg + " to queue");
                    // this allows the driver to respond rapidly but long 
                    // commands to take their time in the right order for multiple clients. 
                    cmdQueue.Enqueue(new CmdTuple(inCmd, cmdArg));
                    //If the command queue is empty, then call the execute directly, otherwise we have to wait for the timer handler to go off. 
                    //the problem with this is that we might call it and the timer might call it at the same time, address that in the execute() cmd.
                    if (cmdQueue.Count == 1)
                    {
                        if (!executeAsyncCmd())
                        {
                            throw new DriverException("syncCommand::Failed to execute async driver command - check logs");
                        }
                    }
                    else
                        output = true;
                }
            }
            return output;
        }

        /*
         * Helper function to return current slewing state. 
         * Needs to check the queue as well, or programs might see the gaps between timer 
         *
        public bool checkSlewing()
        {
            bool output = false;
            if (domeState == DomeImpl.DomeStates.SLEW_CW || domeState == DomeImpl.DomeStates.SLEW_CCW ||
                domeState == DomeImpl.DomeStates.SLEW_CW_TIMED || domeState == DomeImpl.DomeStates.SLEW_CCW_TIMED ||
                (cmdQueue.Count > 0) )
                {
                    output = true;
                }
            return output;
        }
         */
        
        /*
         * Helper function to turn setting properties into generic hash table object so user can update settings and config can be updated. 
         */ 
        protected void createConfigMap( Hashtable configMap, portType port )
        {
            string[] parsedComms;
            int baud;
            int stopBits;
            int length;
            bool success = false;
                
            parsedComms = Properties.Settings.Default.DomeCommSetting.Split(',');
            success = int.TryParse( parsedComms[0], out baud );
            if (success)
            {
                switch (baud)
                {
                    case 9600: configMap["BaudRate"] = SerialSpeed.ps9600; break;
                    case 57600: configMap["BaudRate"] = SerialSpeed.ps57600; break;
                    case 19200: configMap["BaudRate"] = SerialSpeed.ps19200; break;
                    default: configMap["BaudRate"] = SerialSpeed.ps9600; break;
                }
            }
                
            //Simple Parity
            if ( parsedComms[1].CompareTo("n") == 0 )
                configMap["Parity"] = SerialParity.None;
            else if (parsedComms[1].CompareTo("o") == 0 )
                configMap["Parity"] = SerialParity.Odd;
            else if(parsedComms[1].CompareTo("m") == 0 )
                configMap["Parity"] = SerialParity.Mark;
            else 
                configMap["Parity"] = SerialParity.Even;

            //Simple data length
            success = int.TryParse( parsedComms[2], out length );
            configMap["Length"] =  length;
                
            //Stop bits
            success = int.TryParse( parsedComms[3], out stopBits );
            configMap["StopBits"] = stopBits;

            if (port == portType.DOME_PORT)
            {
                configMap["portId"] = Properties.Settings.Default.DomeCommPort;
                configMap["ProxyAddress"] = Properties.Settings.Default.I2CDomeProxyAddr;
            }
            else if (port == portType.OBBO_PORT)
            {
                configMap["portId"] = Properties.Settings.Default.ObboCommPort;
                configMap["ProxyAddress"] = Properties.Settings.Default.I2CObboProxyAddr;
            }
        }

        /*
         * getter/setter for connected state. 
         * If already connected, leave alone
         * if not already connected and 
         */
        public ConnectionState IsConnected
        {
            get { return eConnected; }
            set
            {
                Hashtable configMap;
                try
                {
                    switch (value)
                    {
                        case ConnectionState.CONNECTED:
                            if (eConnected == ConnectionState.CONNECTED)
                                DomeImpl.logger.LogMessage(this.GetType().ToString(), "IsConnected::setting 'connected' when already connected - ignoring");
                            else if (domePort != null && obboPort != null)
                            {
                                //assume the connection is still live
                                if (domePort.Connect(DomeImpl.ConnectionState.CONNECTED) == DeviceComms.CommsConnectionState.connected &&
                                    obboPort.Connect(DomeImpl.ConnectionState.CONNECTED) == DeviceComms.CommsConnectionState.connected)
                                {
                                    obboPort.ClearScreen();
                                    //obboPort.SetCursor(2, 0, 5);
                                    //Update clock on display
                                    //obboPort.WriteLCD(2, 0, System.DateTime.UtcNow.ToShortTimeString());

                                    //Update dome status on display
                                    //obboPort.WriteLCD(3, 0, "Dome state: " + DomeStateStrings[(int)domeState]);

                                    //Get first readings
                                    this.azimuth = domePort.GetBearing();
                                    this.voltage = domePort.GetVoltage();
                                }
                            }
                            else
                            {
                                //watch out for potential error of adding things more than once with different addresses
                                String port;
                                    
                                port = Properties.Settings.Default.DomeCommPort;
                                port = port.Substring(3);
                                Int16 commPort = Int16.Parse(port);
                                domePort = new I2CSerialComms(commPort);
                                configMap = new Hashtable();
                                createConfigMap(configMap, portType.DOME_PORT);
                                domePort.UpdateConnectionConfig(configMap);
                                if (Properties.Settings.Default.I2CMagnetometerAddr != 0)
                                    domePort.addSubDevice("Magnetometer", Properties.Settings.Default.I2CMagnetometerAddr);
                                //if (!String.IsNullOrEmpty(Properties.Settings.Default.VoltmeterAddr))
                                // domePort.addSubDevice( "Voltmeter",  0x00 ); //internal interface to adapter
                                domePort.setProxyAddr(Properties.Settings.Default.I2CDomeProxyAddr);

                                port = Properties.Settings.Default.ObboCommPort;
                                port = port.Substring(3);
                                commPort = Int16.Parse(port);
                                obboPort = new I2CSerialComms(commPort);
                                configMap = new Hashtable();
                                createConfigMap(configMap, portType.OBBO_PORT);
                                obboPort.UpdateConnectionConfig(configMap);
                                if (Properties.Settings.Default.I2CMotorCtrlAddr != 0)
                                    obboPort.addSubDevice("MotorController", Properties.Settings.Default.I2CMotorCtrlAddr);
                                if (Properties.Settings.Default.I2CLCDAddr != 0)
                                    obboPort.addSubDevice("LCDDisplay", Properties.Settings.Default.I2CLCDAddr);
                                obboPort.setProxyAddr(Properties.Settings.Default.I2CObboProxyAddr);

                                if (domePort.Connect(DomeImpl.ConnectionState.CONNECTED) == DeviceComms.CommsConnectionState.connected &&
                                       obboPort.Connect(DomeImpl.ConnectionState.CONNECTED) == DeviceComms.CommsConnectionState.connected)
                                {
                                    //Configure devices
                                    domePort.InitMagnetometerDevice();
                                    obboPort.InitMotorDevice();
                                    obboPort.ClearScreen();
                                    obboPort.SetCursor(2, 0, 5);

                                    //Get first readings
                                    this.azimuth = domePort.GetBearing();
                                    this.voltage = domePort.GetVoltage();

                                    //enable the async timer handler
                                    aTimer.Elapsed += OnTimedEvent;
                                    aTimer.Enabled = true;
                                    eConnected = ConnectionState.CONNECTED;
                                    DomeImpl.logger.LogMessage(this.GetType().ToString(), "IsConnected:: Connect Setting 'connected', devices initialised and timer handler started");
                                }
                                else
                                {
                                    DomeImpl.logger.LogMessage(this.GetType().ToString(), "IsConnected:: Connect Setting to 'partial' - not valid");
                                    eConnected = ConnectionState.PARTIAL_CONNECT;
                                    break;
                                }
                            }
                            break;
                        case ConnectionState.NOT_CONNECTED:
                            DomeImpl.logger.LogMessage(this.GetType().ToString(), "IsConnected:: Connect setting to 'not connected' ");
                             if (this.eConnected != ConnectionState.NOT_CONNECTED)
                             {
                                 //Close the dome down in a friendly way ?
                                     //i.e. Move to parked position
                                     //Close shutters
                                     //Issue STOP command.
                                     //clear down the pending action queue. 
                                 syncCommand( DomeCmds.ABORT, 0.0);

                                 //disable the async timer handler ?
                                 DomeImpl.logger.LogMessage(this.GetType().ToString(), "IsConnected:: Timer tick disabled due to 'not connected' ");
                                 aTimer.Enabled = false;
                                 aTimer.Elapsed -= OnTimedEvent;
                                 //Wait out a timer tick to ensure it is stopped correctly
                                 System.Threading.Thread.Sleep(1000);

                                 //Should we really disconnect the dome from its hardware here ? 
                                 //Is it safe to assume this is a private interface ? 
                                 this.domePort.InitMotorDevice();
                                 domePort.Connect(DomeImpl.ConnectionState.NOT_CONNECTED);
                                 obboPort.Connect(DomeImpl.ConnectionState.NOT_CONNECTED);
                                 domePort = null;
                                 obboPort = null;
                                 eConnected = ConnectionState.NOT_CONNECTED;
                             }
                             break;
                        case ConnectionState.PARTIAL_CONNECT:
                             DomeImpl.logger.LogMessage(this.GetType().ToString(), "IsConnected:: Connect Setting to 'partial' - not valid");
                            eConnected = ConnectionState.PARTIAL_CONNECT;
                            break;
                        case ConnectionState.RESET:
                            //domePort.
                            DomeImpl.logger.LogMessage(this.GetType().ToString(), "IsConnected:: Connect setting to 'reset'. Re-setting devices is not possible. Trying to re-connect");
                            domePort.Connect(DomeImpl.ConnectionState.RESET);
                            obboPort.Connect(DomeImpl.ConnectionState.RESET);
                            eConnected = ConnectionState.RESET;
                            break;
                        default:
                            break;
                    }
                }
                catch (ASCOM.NotConnectedException dnce)
                {
                    value = ConnectionState.NOT_CONNECTED;
                    logger.LogMessage(this.GetType().ToString(), "IsConnected:: Did Not Connect Exception setting new connection state. Current is: " + eConnected + " and desired is:" + value);
                    
                    //Clean up for next attempt
                    domePort.Connect(ConnectionState.RESET);
                    obboPort.Connect(ConnectionState.RESET);
                    throw (dnce);
                }
            }
        }

        #endregion

        #region private
        
        //function to check connected devices, assumes we have already successfully opened serial ports to both. 
        //Due to the motor control check- we should not call this during slewing actions. 
        //Should really call this occasionally from the timer or risk calling azimuth twice from two locations. etc.
        private ConnectionState ConnectDevices( byte checkMask = 0x0f)
        {
            int retVal = 0;
            ConnectionState output;

            logger.LogMessage(this.GetType().ToString(), "Entered ConnectDevices().");
            try
            {
                //Connect to Dome controller
                //Check it's responding then check sub devices.
                //battery monitor 
                if ( (checkMask & 0x01) > 0 )
                    if (domePort.CheckVoltageDevice())
                    {
                        logger.LogMessage(this.GetType().ToString(), "Dome voltage obtained OK.");
                        retVal |= 0x01;
                    }
                
                //Magnetometer               
                if ((checkMask & 0x02) > 0)
                    if( domePort.CheckMagnetometerDevice())
                    {
                        logger.LogMessage(this.GetType().ToString(), "Dome magnetometer contacted OK.");
                        retVal |= 0x02;
                    }

                //Connect to obbo Controller
                //Check it's responding then check sub devices. 
                //Due to the motor control check- we should not call this during slewing actions. 
                //motor controller
                if ((checkMask & 0x4 ) > 0)
                    if (obboPort.CheckMotorDevice())
                    {
                        logger.LogMessage(this.GetType().ToString(), "Motor controller contacted ok.");
                        retVal |= 0x04;
                    }
                
                //LCD                         
                if ((checkMask & 0x08) > 0)
                    if (obboPort.CheckLCDDevice())
                    {
                        logger.LogMessage(this.GetType().ToString(), "LCD Controller contacted ok.");
                        retVal |= 0x08;
                    }
                
                //Temp sensor
                //retVal =| 0x10;
                //Humidity sensor           
                //retVal |= 0x20; etc
            }
            catch (SystemException seEx)
            {
                logger.LogMessage(this.GetType().ToString(), "Exception thrown during device comms checks" + seEx.Message);
            }
            finally
            {
                if (retVal == checkMask )
                    output = ConnectionState.CONNECTED;
                else if ( (retVal & checkMask) > 0 )
                    output = ConnectionState.PARTIAL_CONNECT;
                else
                    output = ConnectionState.NOT_CONNECTED;
            }
            logger.LogMessage(this.GetType().ToString(), "Exiting ConnectDevices() status:" + retVal + " compared to desired: " + checkMask);
            return output;
        }
        #endregion
    }
    
    //Simplifying interface calls for basic devices attached through the comms device layer
    //Expect another motor interface instance for controlling the shutter motor on the rotating dome. 
    //This also drives the need for a microswitch interface to monitor the limit switches. That will use a PF8514 i2C port expander and 
    //monitor individual bits for the switch closures/openings.

    interface LCDDevice
    {
        bool CheckLCDDevice();
        void ClearScreen();
        void SetCursor(int row, int col, int cursorMode);
        void WriteLCD(int row, int col, String words);
    }

    interface PortExpander
    {
        void setupPort();
        int readPort();
        void writePort(byte data);
    }

    interface BatteryDevice
    {
        bool CheckVoltageDevice();
        float GetVoltage();
    }

    interface MagnetometerDevice
    {
        bool CheckMagnetometerDevice();
        void InitMagnetometerDevice();
        float GetBearing();
    }

    interface MotorDevice
    {
        bool CheckMotorDevice();
        void InitMotorDevice();
        bool SetSpeedDirection(byte speed, bool forwards );
    }
    
    public abstract class DeviceComms : LCDDevice, BatteryDevice, MagnetometerDevice, MotorDevice
    {
        //This class acts as the superclass for the device access class family. 
        //Each form of access device (i2c, RS232, rs485, SPI etc should implement a derivative of this class
        //
        protected Object localLock = new Object();
        protected int writeLength;
        protected int readLength;
        protected int targetAddr;
        protected char[] databuff;
        protected int numSubDevices = 0;
        protected int interfaceId;
        protected int proxyAddress = 0;
        protected System.Collections.Hashtable deviceMap;

        //These functions need implementing in the derived class. 
        public enum CommsConnectionState { connected, disconnected, partial, reset };
        public abstract CommsConnectionState Connect( DomeImpl.ConnectionState state);// throw NotConnectedException;
        public abstract bool UpdateConnectionConfig(System.Collections.Hashtable configMap);

        public abstract int Read(byte proxyAddress,
                            byte address,
                            byte command,
                            out byte[] outData,
                            byte length);
        public abstract int Write(byte proxyAddress, byte targetAddress, byte register, byte length, byte[] data);

        //Interface functions
        public abstract bool CheckLCDDevice();
        public abstract void ClearScreen();
        public abstract void SetCursor(int row, int col, int cursorMode);
        public abstract void WriteLCD(int row, int col, String words);
        
        public abstract bool CheckMotorDevice();   
        public abstract void InitMotorDevice();   
        public abstract bool SetSpeedDirection( byte speed, bool forwards );

        public abstract bool CheckMagnetometerDevice();
        public abstract void InitMagnetometerDevice();
        public abstract float GetBearing();
        
        public abstract bool CheckVoltageDevice();
        public abstract float GetVoltage();

        public void setProxyAddr( int address )
        {
            if (address < 1024 && address > 0)
            {
                this.proxyAddress = address;
                DomeImpl.logger.LogMessage(this.GetType().ToString(), "setProxyAddr::i2c proxy address - accepted: " + address);
            }
            else
            {
                this.proxyAddress = 0;
                DomeImpl.logger.LogMessage(this.GetType().ToString(), "setProxyAddr::i2c proxy address - invalid: " + address);
            }
        }

        public DeviceComms()
        {
            DomeImpl.logger.LogMessage(this.GetType().ToString(), "DeviceComms::Creating device interface");
            //Get the config
            //read the flat set and parse for devices accessed through this hardware interface
            //store into subdevices by address as id
            interfaceId = 0;
            databuff = new char[256];
            deviceMap = new Hashtable();
        }
        
        public DeviceComms(int deviceId)
        {
            DomeImpl.logger.LogMessage(this.GetType().ToString(), "DeviceComms::Creating device interface");
            //Get the config
            //read the flat set and parse for devices accessed through this hardware interface
            //store into subdevices by address as id
            interfaceId = deviceId;
            databuff = new char[256];
            deviceMap = new Hashtable();
        }

        /// <summary>
        /// Add sub devices by specifying a device ame and string
        /// Add functions to implment the devices by specifying an interface and adding to the class. 
        /// </summary>
        /// <param name="deviceName">String descriptive name</param>
        /// <param name="deviceAddress">int i2c address greater than 0 and less than 1023</param>
        public void addSubDevice( String deviceName, int deviceAddress)
        {
            if (deviceAddress >= 0 && deviceAddress <= 1023)
            {
                deviceMap.Add(deviceName, deviceAddress);
                DomeImpl.logger.LogMessage(this.GetType().ToString(), "addSubDevice::Added device type: " + deviceName + " with address: " + deviceAddress + " to commport device map on port: " + interfaceId);
            }
            else
                DomeImpl.logger.LogMessage(this.GetType().ToString(), "addSubDevice::Failed to add device type: " + deviceName + " with address: " + deviceAddress + " to commport device map on port: " + interfaceId);
        }
        
        private void Dispose()
        {
            DomeImpl.logger.LogMessage(this.GetType().ToString(), "addSubDevice::closing connection and cleaning up");          
            deviceMap.Clear();
        }
    }

    //This class implements access via the serial ports to the I2C interfaces hosted on the remote controllers.
    //There are two different i2c buses - local and dome so there needs to be two instances of this class at the execution level. 
    //Access to read and write is mutexed to prevent re-entrancy through multiple clients talking at the same time.
    public class I2CSerialComms : DeviceComms
    {
        public const int I2C_RD = 1;
        public const int I2C_WR = 0;
        
        private ASCOM.Utilities.Serial commPort = new Serial();

        #region public properties and methods
        
        //constructor with a pre-prepared serial port
        public I2CSerialComms(int interfaceId) : base(interfaceId)
        {
            localLock = new Object();
            DomeImpl.logger.LogMessage(this.GetType().ToString(), "I2CSerialComms::Constructor called with port Id");
            if (interfaceId != 0)
            {
                commPort.Port = interfaceId;
            }
            else
                throw new ASCOM.NotConnectedException("I2CSerialComms::Unable to identify device ID" + interfaceId);
        }
        
        //constructor without a pre-prepared serial port
        public I2CSerialComms()
        {
            DomeImpl.logger.LogMessage(this.GetType().ToString(), "I2CSerialComms::Default Constructor called");
            interfaceId = 0;
        }

        public override bool UpdateConnectionConfig( System.Collections.Hashtable configMap )
        {
            DomeImpl.logger.LogMessage(this.GetType().ToString(), "UpdateConnectionConfig:: called");
            localLock = new Object();
            
            if (configMap.Count == 0)
                return false;

            //int baud, bool parity, int length,  int stopBits)
            if( interfaceId != 0 ) 
                if (commPort.Connected ) 
                    commPort.Connected = false;
            
            //Get config from hashmap
            if (configMap.ContainsKey("Handshake"))
                commPort.Handshake = (SerialHandshake)configMap["Handshake"];
            else
                commPort.Handshake = SerialHandshake.None;

            if ( configMap.ContainsKey("Parity"))
                commPort.Parity = (SerialParity) configMap["Parity"];
            else commPort.Parity = SerialParity.None;
            
            if ( configMap.ContainsKey("StopBits"))
                commPort.StopBits = (SerialStopBits) configMap["StopBits"];
            else 
                commPort.StopBits = SerialStopBits.One;
            
            if ( configMap.ContainsKey("Length"))
                commPort.DataBits = (int) configMap["Length"];
            else 
                commPort.DataBits = 7;
                               
            if ( configMap.ContainsKey("BaudRate"))
                commPort.Speed = (SerialSpeed) configMap["BaudRate"];
            else
                commPort.Speed = SerialSpeed.ps9600;

            if ( configMap.ContainsKey( "ReceiveTimeout"))
                commPort.ReceiveTimeoutMs = (int) configMap["ReceiveTimeout"];
            else
                commPort.ReceiveTimeoutMs = 250;

            /*
             * if (configMap.ContainsKey("PortId"))
                commPort.Port = (int)configMap["PortId"];
            else
                ;//No fall back position.
             * */

            if( configMap.ContainsKey( "CommPort"))
            {
                commPort.Port = (int) configMap["CommPort"];
                interfaceId = commPort.Port;
            }

            //These can be set externally as forced variable in ASCOM comm port settings. Set here instead.
            //
            commPort.DTREnable = false;
            commPort.RTSEnable = false;
            //Setup comport settings
            /*
             * commPort.Parity = SerialParity.None;
            commPort.Handshake = SerialHandshake.None;
            commPort.DTREnable = false;
            commPort.DataBits = 8;
            commPort.StopBits = SerialStopBits.One;
            commPort.Speed = SerialSpeed.ps19200;
            commPort.ReceiveTimeoutMs = 250;
            */
            DomeImpl.logger.LogMessage(this.GetType().ToString(), "UpdateConnectionConfig:: connection config updated in Dome driver");
            return true;
        }

        private void Dispose()
        {
            Connect(DomeImpl.ConnectionState.NOT_CONNECTED);
            commPort.ClearBuffers();
            commPort.Connected = false;
            DomeImpl.logger.LogMessage(this.GetType().ToString(), "Dispose:: comm port" + commPort.Port + " disposed in Dome driver");
            this.commPort.Connected = false;
            //super.Dispose();
        }

        // Tech Specs: http://www.robot-electronics.co.uk/htm/Lcd05tech.htm
        public override bool CheckLCDDevice()
        {
            bool outStatus = true;
            byte[] inData;// = new byte[1];

            //Read battery voltage attached to proxy host
            if (proxyAddress != 0)
            {
                //Note - LCD read will contain no usable data  - its a no-op. 
                if (Read((byte)proxyAddress, (byte)Properties.Settings.Default.I2CLCDAddr, (byte)0x00, out inData, 4) > 0)
                {
                    DomeImpl.logger.LogMessage(this.GetType().ToString(), "CheckLCDDevice::LCD check OK");
                }
                else
                {
                    DomeImpl.logger.LogMessage(this.GetType().ToString(), "CheckLCDDevice::LCD check failed");
                    outStatus = false;
                }
            }
            return outStatus;
        }

        public override void WriteLCD(int row, int col, String letters )
        {
            byte[] outData;
            char[] charString;
            int dataCount = 0;
            
            //Move to row/col
            outData = new byte[3];
            outData[1] = (byte)(row); //data
            outData[2] = (byte)(col); //data
            dataCount = Write( (byte)proxyAddress, (byte)Properties.Settings.Default.I2CLCDAddr, 0x00, 3, outData );

            //Write string to device display
            charString = letters.ToCharArray();
            outData = new byte[charString.Length];
            for (int i = 0; i < charString.Length; i++)
            {
                outData[i] = (byte)charString[i];
            }
            dataCount = Write((byte)proxyAddress, (byte)Properties.Settings.Default.I2CLCDAddr, 0x00, (byte) outData.Length, outData);

        }

        public override void ClearScreen()
        {
            byte[] outData;

            outData = new byte[1];
            outData[0] = (byte) 12;//CLS
            Write((byte)proxyAddress, (byte)Properties.Settings.Default.I2CLCDAddr, 0x00, 1, outData);
        }
        
        public override void SetCursor(int row, int col, int cursorMode)
        {
            byte[] outData;
            int dataCount = 0;

            //Move to row/col
            outData = new byte[3];
            outData[0] = (byte) 3;
            outData[1] = (byte) row;
            outData[2] = (byte) col; //data
            dataCount = Write((byte)proxyAddress, (byte)Properties.Settings.Default.I2CLCDAddr, 0x00, 3, outData);

            //Set cursor mode to solid, blink or underline
            outData = new byte[1];
            switch (cursorMode)
            {
                case 0: outData[0] = 0x04;
                    break;
                case 1: outData[0] = 0x05;
                    break;
                case 2: outData[0] = 0x06;
                    break;
                default: outData[0] = 0x04;
                    break;
            }
            dataCount = Write((byte)proxyAddress, (byte)Properties.Settings.Default.I2CLCDAddr, 0x00, 1, outData);
        }

        public override bool CheckVoltageDevice()
        {
            bool outStatus = false;
            byte[] inData = new byte[2];

            //Read battery voltage attached to proxy host
            if (proxyAddress != 0 )
            {
                //Note --fixedup address to remote wireless interface base address.
                //Uses a different proxy address - need to fixup settings in general for this. 
                if (Read((byte) 0, (byte)0x5a, (byte) 0x01, out inData, 1 ) > 0)
                {
                    DomeImpl.logger.LogMessage(this.GetType().ToString(), "Voltage check - version is: " + inData[0].ToString() );
                }
                else
                {
                    DomeImpl.logger.LogMessage(this.GetType().ToString(), "Voltage device comms check failed ");
                    outStatus = false;
                }
            }
            return outStatus;
        }

        //Call to check battery voltage reading from proxy radio interface used on the rotating dome 
        //Robot electroncs device RF04/CM02 is the device required from RobotElectronics
        // http://www.robot-electronics.co.uk/htm/cm02tech.htm
        public override float GetVoltage()
        {
            byte[] inData = new byte[2];
            float voltage = 0.0f;
           
            //Read battery voltage attached to proxy host
            //Note --fixup address to remote wireless interface base address.
            Read( (byte)0x5a, (byte)0, (byte) 0x03, out inData, 2 );
            voltage = (inData[1] + inData[0] * 256.0f)/4198;

            DomeImpl.logger.LogMessage( this.GetType().ToString(), "voltage is: " + voltage);
            return voltage;
        }

        // http://www.robot-electronics.co.uk/htm/cmps3tech.htm
        //Replaced with HMC5883 @ 0x36
        public override bool CheckMagnetometerDevice()
        {
            bool outStatus = false;
            int count = 0;
            byte[] inData = new byte[1];
            String version;

            //Read magnetometer version string from device at register 0
            if (proxyAddress != 0 )
            {    //Reg 8 provides a read only status value on the HMC5883 sensor
                count = Read( (byte)proxyAddress,
                        (byte)Properties.Settings.Default.I2CMagnetometerAddr,
                        (byte)0x08,
                        out inData,
                        1);

                if (count > 0)
                {
                    version = inData[0].ToString();
                    DomeImpl.logger.LogMessage(this.GetType().ToString(), "Magnetometer check - status read successfully: " + version);
                    outStatus = true;
                }
                else
                {
                    DomeImpl.logger.LogMessage(this.GetType().ToString(), "Magnetometer connection check failed");
                    outStatus = false;
                }
            }
            return outStatus;
        }

        //Do device setup here
        //Only applies to HMC5883
        public override void InitMagnetometerDevice()
        {
            int I2CDeviceAddress = Properties.Settings.Default.I2CMagnetometerAddr;
            byte[] outByte;
            int proxyAddress = Properties.Settings.Default.I2CDomeProxyAddr;
            int dataCount = 0;
#if HMC5883_MAGNETOMETER
            //I2CDeviceAddress = 0x3d;
                        
            //Need to enable magnetometer and set the gain
            //send command - 0 to msb of zero register.
            outByte = new byte[1];
//            outByte[0] = (byte) proxyAddress;                 //The proxy is a serial to I2C device
//            outByte[1] = (byte)(I2CDeviceAddress & 0xFE | I2C_WR );     //I2C write needs LSB clear
//            outByte[0] = (byte) 0;                            // device register to write
//            outByte[1] = 1;                                   //length of data to write
            outByte[0] = 0x40;                                //Data value to write

            dataCount = Write((byte)proxyAddress, (byte)Properties.Settings.Default.I2CMagnetometerAddr, 0x00, 1, outByte );
            if ( dataCount > 0 )
            {
                DomeImpl.logger.LogMessage(this.GetType().ToString(), "Wrote magnetometer config byte succesfully");
            }
                        
            //send command mode->continuous sensing
//            outByte[0] = proxyAddress;                        //The proxy is a serial to I2C device
//            outByte[1] = (byte)(I2CDeviceAddress & 0xFE);     //I2C write needs LSB clear
//            outByte[0] = (byte)2;                               //Device register to read
//            outByte[1] = 1;                                     //length of data to read
            outByte[0] = 0x00;

            dataCount = Write((byte)proxyAddress, (byte)Properties.Settings.Default.I2CMagnetometerAddr, 0x02, 1, outByte);
            if (dataCount > 0)
            {
                DomeImpl.logger.LogMessage(this.GetType().ToString(), "Wrote magnetometer continuous sensing config byte succesfully");
            }
#endif
        }
        
        //Call to obtain double precision bearing reading - 3600 is 360 degrees.
        //Originally used Robot electronics device CMPS03.
        //Replaced with HoneywellHMC5883 device
        public override float GetBearing()
        {
            byte[] inData = new byte[2];
            float bearing = 0.0f;

            //Read battery voltage attached to proxy host
            if (proxyAddress != 0 )
            {

#if DEVASYS_MAGNETOMETER                
                Read((byte)proxyAddress,
                        (byte)Properties.Settings.Default.I2CMagnetometerAddr,
                        (byte)0x02,
                        out inData,
                        2);
                bearing = (float) (inData[0] + (inData[1] << 8 ));
#endif 
#if HMC5883_MAGNETOMETER
                Read((byte)proxyAddress,
                        (byte)Properties.Settings.Default.I2CMagnetometerAddr,
                        (byte)0x03,
                        out inData,
                        6);
                if (inData.GetLength(0) > 0)
                {
                    int x, y, z;
                    String outString;
                    //Seems c# can't handle conversion of unsigned two-complement bytes to signed ints properly  - or I can't.
                    x = (inData[0] << 8) | inData[1];
                    x = ((inData[0] & 0x80) != 0) ? x - 65536 : x;
                    y = (inData[4] << 8) | inData[5];
                    y = ((inData[4] & 0x80) != 0) ? y - 65536 : y;
                    z = (inData[2] << 8) | inData[3];
                    z = ((inData[2] & 0x80) != 0) ? z - 65536 : z;

                    //aTAN2 returns a bearing where 0 lies along the x axis.
                    bearing = (float)((180.0 / Math.PI) * Math.Atan2(y, x));
                    bearing = (bearing + 360) % 360;

                    outString = String.Format("GetBearing:: X x[1]x[0] {0:x} {1:x} {2:D4}", inData[0], inData[1], x);
                    DomeImpl.logger.LogMessage(this.GetType().ToString(), outString);
                    outString = String.Format("GetBearing:: Y y[1]y[0] {0:x} {1:x} {2:D4}", inData[4], inData[5], y);
                    DomeImpl.logger.LogMessage(this.GetType().ToString(), outString);
                    outString = String.Format("GetBearing:: Z z[1]z[0] {0:x} {1:x} {2:D4}", inData[2], inData[3], z);
#endif
                }
                DomeImpl.logger.LogMessage(this.GetType().ToString(), "GetBearing:: Bearing is: " + bearing);

            }
            return bearing;
        }

        /*
         * Valid input values of speed are from 1 to 255;
         * Valid input values of direction are False - reverse, True - forwards 
         */
        public override bool SetSpeedDirection( byte speed, bool forwards )
        {
            bool validCmd = false;
            int writeState = 0;
            //Write string to device
            byte[] outData = new byte[2];
            
            //Convert speed and direction into simple velocity
            //motor mode register address - mode 1 is 0 (full reverse)  128 (stop)   255 (full forward).
            //set both speed registers - motor1 and motor2

            if (speed == 0)
            {
                speed = 128;
            }
            else
            {
                speed = (byte)(speed/2 );
                if (forwards)
                    speed = (byte)( 128 - speed);
                else
                    speed = (byte)( 128 + speed);
            }
                            
            outData[0] = (byte)speed; //speed value motor 1;
            outData[1] = (byte)speed; //speed value motor 2;
            writeState = Write((byte)proxyAddress, (byte)Settings.Default.I2CMotorCtrlAddr, 0x01, 2, outData);
            if ( writeState != 0)
            {
                validCmd = true;
                DomeImpl.logger.LogMessage(this.GetType().ToString(), String.Format("SetSpeedDirection:: Speed set to {0} & direction set to {1}: ", speed, forwards));
            }
            else
                DomeImpl.logger.LogMessage(this.GetType().ToString(), String.Format("SetSpeedDirection:: Failed to set Speed to {0} & direction set to {1}: ", speed, forwards));
            
            return validCmd;
        }

        //
        public override bool CheckMotorDevice()
        {
            //Write string to device
            byte[] outData = new byte[1];
            bool outStatus = false; 
            outData[0] = 7; //Version register address

            if (Read((byte)proxyAddress, (byte)Settings.Default.I2CMotorCtrlAddr, 0, out outData, 1) > 0)
            {
                DomeImpl.logger.LogMessage(this.GetType().ToString(), "CheckMotorDevice::Dome motor version: " + outData[0].ToString());
                outStatus = true;
            }
            else
            {
                DomeImpl.logger.LogMessage(this.GetType().ToString(), "CheckMotorDevice::Dome motor unable to contact");
                outStatus = true;
            }
            return outStatus;
        }

        public override void InitMotorDevice()
        {
            //Write string to device
            int responseCount = 0;
            byte[] outData = new byte[4];
            outData[0] = 0; //motor mode register address - mode 1 is 0 (full reverse)  128 (stop)   255 (full forward).
            outData[1] = 0; //both motors are controlled by their own speed registers, speed register motor 1
            outData[2] = 0; //speed register motor 2
            outData[3] = 255; //slow acceleration
            
            responseCount = Write((byte)proxyAddress, (byte)Settings.Default.I2CMotorCtrlAddr, 0x00, 4, outData);
            if (  responseCount> 0 )
                DomeImpl.logger.LogMessage(this.GetType().ToString(), "InitMotorDevice::Dome motors initialised");
            else
                DomeImpl.logger.LogMessage(this.GetType().ToString(), "InitMotorDevice::Dome motors init - no response");
        }

        //this functions checks the comms port is connected and then checks the sub devices for each port is connected.
        //update the config before calling CONNECT to ensure valid configuration state and 
        // close devices before DISCONNECTING to ensure clean closure 
        public override CommsConnectionState Connect( DomeImpl.ConnectionState conn)
        {
            CommsConnectionState output = CommsConnectionState.disconnected;          
            CommsConnectionState current = CommsConnectionState.disconnected;          ;

            DomeImpl.logger.LogMessage(this.GetType().ToString(), String.Format( "Connect:: Called for {0} with arg {1}", commPort.Port, (int) conn ));
            
            if (this.commPort.Port == 0)
            {//Commport not assigned - need to fix. Shouldn't be possible to get here without it being set - its not a user error.
                DomeImpl.logger.LogMessage(this.GetType().ToString(), "Connect::i2c comm port" + commPort.Port + " not configured for comm port yet in Dome Driver");
                throw new System.Exception("Comm port not set");
                //return output;
            }
           
            //Need to add exception handling around commPort commands
            //CONNECTED means we are being asked to connect..
            if (conn == DomeImpl.ConnectionState.CONNECTED)
            {              
                if (this.commPort.Port != 0 && this.commPort.Connected)
                {//Already connected - nothing to do
                    DomeImpl.logger.LogMessage(this.GetType().ToString(), "Connect::i2c comm port" + commPort.Port + " already connected in Dome Driver");
                }
                else
                {
                    this.commPort.Connected = true;
                    if (deviceMap.Count > 0)
                    {
                        IEnumerator ie = deviceMap.Keys.GetEnumerator();
                        int[] len = new int[deviceMap.Keys.Count];
                        int devIndex = 0;
                        int sumDevices = 0;
                        String devName;
                        ie.MoveNext();
                        do
                        {
                            devName = (String) (ie.Current);
                            if (devName.CompareTo("Magnetometer") == 0)
                            {
                                len[devIndex] = (CheckMagnetometerDevice() == true) ? 1 : 0;
                                sumDevices += 2 ^ (devIndex);
                            }
                            else if (devName.CompareTo("LCDDisplay") == 0)
                            {
                                len[devIndex] = (CheckLCDDevice() == true) ? 1 : 0;
                                sumDevices += 2 ^ (devIndex);
                            }
                            else if (devName.CompareTo("MotorController") == 0)
                            {
                                len[devIndex] = (CheckMotorDevice() == true) ? 1 : 0;
                                sumDevices += 2^(devIndex);
                            }
                            /*
                            else if (devName.CompareTo("Voltmeter") == 0)
                            {
                                len[devIndex] = (CheckVoltageDevice() == true) ? 1 : 0;
                                sumDevices += 2^(devIndex);
                             }
                             * 
                             */
                            ie.MoveNext();
                            devIndex++; 
                        } while (devIndex < deviceMap.Count );
                    
                        if ( sumDevices < (2^devIndex)-1 )
                            current = CommsConnectionState.partial;
                    }

                    if (current == CommsConnectionState.partial)
                    {
                        //Partial is not a good state, the checks will indicate which device is not responding
                        DomeImpl.logger.LogMessage(this.GetType().ToString(), "Connect:: partial state found - check logs");
                        output = current;
                    }
                    else
                    {
                        DomeImpl.logger.LogMessage(this.GetType().ToString(), "Connect::i2c comm port" + this.commPort.Port + " connected in Dome Driver");
                        output = CommsConnectionState.connected;
                    }
                }
            }
            else if (conn == DomeImpl.ConnectionState.NOT_CONNECTED)
            {
                commPort.ClearBuffers();
                commPort.Connected = false;
                commPort.LogMessage(this.GetType().ToString(), "Connect:: comm port " + commPort.Port + " cleared by Dome driver, not closed for NOT_CONNECTED");
                output = CommsConnectionState.disconnected;
            }
            return output;
        }
        #endregion

        #region private methods

        //Basic write function for virtual interface. 
        //Override based on attached hardware type (i2c, SPI, CAN, MODBUS, bit-banged etc) or not.
        // https://www.robot-electronics.co.uk/htm/usb_iss_tech.htm
        // https://www.robot-electronics.co.uk/htm/cmps3doc.htm
        //Both of these interfaces use the proxy address of 0x55 to query I2C devices with internal registers. 
        public override int Write( byte proxyAddress, byte targetAddress, byte register, byte length, byte[] data)
        {
            byte[] outData;
            byte[] output = new byte[1];
            System.DateTime dt;
            bool timeoutExceeded = false;
            int i = 0, j = 0;
            Object localLock = new Object();

            //grab  mutex or wait
            lock (localLock)
            {
                //Use this as the condition to indicate we want to write some registers on the proxy device itself
                if (proxyAddress > 0 && proxyAddress < 255 && targetAddress == 0)
                {
                    outData = new Byte[data.Length + 3];
                    outData[i++] = (byte)((proxyAddress & 0xFE) | (byte)I2C_WR);
                    outData[i++] = register;
                    outData[i++] = length;
                }                
                //This one is when there is no proxy  - some sort of direct interface
                else if (proxyAddress == 0 && targetAddress > 0 && targetAddress < 255 )
                {
                    outData = new byte[data.Length + 3];
                    outData[i++] = (byte)((targetAddress & 0xFE) | (byte)I2C_WR);
                    outData[i++] = register;
                    outData[i++] = length;
                }
                //This is for using the proxy
                else if ( proxyAddress > 0 && proxyAddress < 255 && 
                          targetAddress > 0 && targetAddress< 255 ) 
                {
                    outData = new byte[data.Length + 4];
                    outData[i++] = proxyAddress;
                    outData[i++] = (byte)((targetAddress & 0xFE) | (byte)I2C_WR);
                    outData[i++] = register;
                    outData[i++] = length;
                }
                //trap the fall-through
                else
                {
                    String formattedString = String.Format("No valid combination of proxy address or i2c target device address found. proxy: {0}, target: {1}, register: {2}", proxyAddress, targetAddress, register);
                    DomeImpl.logger.LogMessage(this.GetType().ToString(), formattedString );
                    return 0;
                }
                
                //Add on the data section
                for (j = 0; j < data.Length; j++)
                    outData[i + j] = data[j];
                
                //dt = System.DateTime.Now.AddMilliseconds(500);
                //do {
                    try
                    {
                        //Write the command to the remote proxy to query the remote device
                        commPort.ClearBuffers();
                        commPort.TransmitBinary(outData);
                        Thread.Sleep(150);
                        output = commPort.ReceiveCountedBinary(1); //Collect status byte
                    }
                    //ASCOM comm port exceptions are reported as COM object external exceptions so not a lot of data available. 
                    catch (ASCOM.Utilities.Exceptions.SerialPortInUseException ex2)
                    {
                        DomeImpl.logger.LogMessage(this.GetType().ToString(), "commport already in use error in DeviceComms.I2CSerialComms.i2CWrite " + ex2.Message);
                    }
                    catch (System.Exception ex1)
                    {
                        DomeImpl.logger.LogMessage(this.GetType().ToString(), "commport error in DeviceComms.I2CSerialComms.i2CWrite " + ex1.Message );                    
                    }
                    //timeoutExceeded = ( ( DateTime.Compare(DateTime.Now, dt) >= 0 ) );
                //}while (!timeoutExceeded && (output[0] == 0)  );
            }
            return (int) output[0];//0 is a failure, 1 or more is success. 
        }

        //Basic read function for virtual interface. Override based on attached hardware type or not. 
        public override int Read(byte proxyAddress, byte address, byte command, out byte[] outData, byte length)
        {
            int i = 0;
            byte[] localOutData;
            byte[] localInData = new byte[length];
            bool timeoutExceeded = false;

            //grab  mutex or wait
            lock (localLock)
            {
                i = 0;
                
                //Use this as the condition to indicate we want to write some registers on the proxy device itself - typically at 0x5a
                if (proxyAddress > 0 && proxyAddress < 255 && address == 0)
                {
                    localOutData = new byte[4]; //Cmds against the proxy itself are always only 4 bytes long
                    localOutData[i++] = (byte)(proxyAddress);//Need to be literal here - doesn't work if you OR in the I2C_RD bit
                    localOutData[i++] = command;
                    localOutData[i++] = 0;
                    localOutData[i++] = 0;
                    //localOutData[i++] = length;
                }
                else
                {
                    localOutData = new byte[4];
                    localOutData[i++] = proxyAddress;
                    localOutData[i++] = (byte)((address & 0xFE) | I2C_RD);
                    localOutData[i++] = command;
                    localOutData[i++] = length;
                }
                
                localInData = new byte[length];
                
                System.DateTime dt = System.DateTime.Now;
                timeoutExceeded = false;
                do
                {
                    try
                    {
                        //send read command to remote proxy
                        commPort.ClearBuffers();
                        commPort.TransmitBinary(localOutData);

                        //Thread.Sleep(150);
                        //read response back from remote proxy
                        localInData = commPort.ReceiveCountedBinary(length);
                    }
                    catch (ASCOM.DriverAccessCOMException cex)
                    {
                        //ASCOM.NotConnectedException, ASCOM.Utilities.Exceptions.SerialPortInUseException
                        if (cex.InnerException != null )
                            commPort.LogMessage(this.GetType().ToString(), "commport error in DeviceComms.I2CSerialComms.i2CRead" + cex.Message + cex.InnerException.Message);
                        else
                            commPort.LogMessage(this.GetType().ToString(), "commport error in DeviceComms.I2CSerialComms.i2CRead" + cex.Message );
                    }
                    catch (System.Exception ex1)
                    {
                        //ASCOM.NotConnectedException, ASCOM.Utilities.Exceptions.SerialPortInUseException
                        if ( ex1.InnerException != null )
                            commPort.LogMessage(this.GetType().ToString(), "commport error in DeviceComms.I2CSerialComms.i2CRead " + ex1.Message + ex1.InnerException.Message);
                        else
                            commPort.LogMessage(this.GetType().ToString(), "commport error in DeviceComms.I2CSerialComms.i2CRead " + ex1.Message );
                    }
                    timeoutExceeded = (DateTime.Compare(System.DateTime.Now, dt) < 0 );
                }
                while ( !timeoutExceeded && localInData.Length != length );

                if (timeoutExceeded)
                    throw new ASCOM.DriverException("Timeouts exceeded, retried, potential other causes - check ASCOM Serial logs");
            }
            //Update the output array 
            outData = localInData;
            return outData.Length;
        }

        #endregion
    }

    public class CmdTuple
    {
        Object tuple_1, tuple_2;
        public CmdTuple(Object tuple_1, Object tuple_2)
        {
            this.tuple_1 = tuple_1;
            this.tuple_2 = tuple_2;
        }

        public DomeImpl.DomeCmds getTuple_1()
        {
            if (this.tuple_1.GetType() == typeof(Skybadger.DomeImpl.DomeCmds))
                return (Skybadger.DomeImpl.DomeCmds)this.tuple_1;
            else
                throw new System.FieldAccessException("Failed to convert object to DomeCmds enum in tuple_1");
        }

        public double getTuple_2()
        {
            if (this.tuple_2.GetType() == typeof(double))
                return (double)this.tuple_2;
            else
                throw new System.FieldAccessException("Failed to convert object to double in tuple_2");
        }

        public void Dispose()
        {
            this.tuple_1 = null;
            this.tuple_2 = null;
        }
    }

    
//end of namespace
}