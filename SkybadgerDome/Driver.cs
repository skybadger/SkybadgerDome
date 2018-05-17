//tabs=4
// --------------------------------------------------------------------------------
// TODO fill in this information for your driver, then remove this line!
//
// ASCOM Dome driver for Skybadger
//
// Description:	Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam 
//				nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam 
//				erat, sed diam voluptua. At vero eos et accusam et justo duo 
//				dolores et ea rebum. Stet clita kasd gubergren, no sea takimata 
//				sanctus est Lorem ipsum dolor sit amet.
//
// Implements:	ASCOM Dome interface version: <To be completed by driver developer>
// Author:		(XXX) Your N. Here <your@email.here>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// dd-mmm-yyyy	XXX	6.0.0	Initial edit, created from ASCOM driver template
// --------------------------------------------------------------------------------
//

// This is used to define code in the template that is specific to one class implementation
// unused code canbe deleted and this definition removed.
#define Dome

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using ASCOM;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using System.Globalization;
using System.Collections;
/*
 * This dome driver uses comm port devices to talk to i2c devices which are the actual doers
 * The devices are located on the rotating dome and the fixed obbo walls.
 * The comm port devices used are the http://robotelectronics.co.uk USB serial-to-i2c interfaces.
 * The rotating dome uses a wireless serial interface from the same location which is proxied at the remote comm port to the local i2c devices.
 * the motors are motionco.co.uk worm-gearbox 12v DC 65 rpm motors.
 * The wheels are banebots orange 2 3/8" diameter on 8mm ID 1/2" OD hex hubs
 * The brackets are home made and mount the motors spring-loaded on linear sliders running on 8mm bolts bolted into a backing plate, 
 *  to apply constant pressure on the driving surface of the dome. 
 * Code is licensed under creative commons 4.0 for your own re-use. 
 * ASCOM 6.3 is required as the platform for implementation.
 * Code was developed unde VC# 10
 * The test app is used to play with this driver.
 */

namespace ASCOM.Skybadger
{
    //
    // Your driver's DeviceID is ASCOM.Skybadger.Dome
    //
    // The Guid attribute sets the CLSID for ASCOM.Skybadger.Dome
    // The ClassInterface/None addribute prevents an empty interface called
    // _Skybadger from being created and used as the [default] interface
    //
    // TODO right click on IDomeV2 and select "Implement Interface" to
    // generate the property amd method definitions for the driver.
    //
    // TODO Replace the not implemented exceptions with code to implement the function or
    // throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Dome Driver for Skybadger.
    /// </summary>
    [Guid("c08c5ffe-ca27-46f6-812f-f183ab9ffdda")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Dome : IDomeV2
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        private static string driverID = "ASCOM.Skybadger.Dome";
        // TODO Change the descriptive string for your driver then remove this line
        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        private static string driverDescription = "ASCOM Dome Driver for Skybadger.";
        
        //Create an instance of the actual driver interface
        private Skybadger.DomeImpl myDome;

#if Telescope
        //
        // Driver private data (rate collections) for the telescope driver only.
        // This can be removed for other driver types
        //
        private readonly AxisRates[] _axisRates;
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="Skybadger"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public Dome()
        {
            //TODO: Implement your additional construction here
            myDome = new DomeImpl( );
        }

        #region ASCOM Registration
        //
        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        /// <summary>
        /// Register or unregister the driver with the ASCOM Platform.
        /// This is harmless if the driver is already registered/unregistered.
        /// </summary>
        /// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        private static void RegUnregASCOM(bool bRegister)
        {
            using (var P = new ASCOM.Utilities.Profile())
            {
                P.DeviceType = "Dome";
                if (bRegister)
                {
                    P.Register(driverID, driverDescription);
                }
                else
                {
                    P.Unregister(driverID);
                }
            }
        }

        /// <summary>
        /// This function registers the driver with the ASCOM Chooser and
        /// is called automatically whenever this class is registered for COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is successfully built.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        /// </remarks>
        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        /// <summary>
        /// This function unregisters the driver from the ASCOM Chooser and
        /// is called automatically whenever this class is unregistered from COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is cleaned or prior to rebuilding.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        /// </remarks>
        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }
        #endregion

        //
        // PUBLIC COM INTERFACE IDomeV2 IMPLEMENTATION
        //

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected
            if (IsConnected)
            {
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");
            }

            using (SetupDialogForm F = new SetupDialogForm())
            {
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    Properties.Settings.Default.Save();
                }
                Properties.Settings.Default.Reload();
            }
        }


        #region common properties and methods. All set to no action

        public System.Collections.ArrayList SupportedActions
        {
            get { return new ArrayList(); }
        }

        public string Action(string actionName, string actionParameters)
        {
            throw new ASCOM.MethodNotImplementedException("Action");
        }

        public void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            // Call CommandString and return as soon as it finishes
            this.CommandString(command, raw);
            // or
            //throw new ASCOM.MethodNotImplementedException("CommandBlind");
        }

        public bool CommandBool(string command, bool raw)
        {
            Boolean output = false;
            CheckConnected("CommandBool");
            string ret = CommandString(command, raw);
            // TODO decode the return string and return true or false
            // or
            //throw new ASCOM.MethodNotImplementedException("CommandBool");
            return output;
        }

        public string CommandString(string command, bool raw)
        {
            String output = "";
            //CheckConnected("CommandString");
            // it's a good idea to put all the low level communication with the device here,
            // then all communication calls this function
            // you need something to ensure that only one command is in progress at a time

            //throw new ASCOM.MethodNotImplementedException("CommandString");
            output = myDome.syncCommand( command );            
            return output;
        }

        #endregion

        #region public properties and methods
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public bool Connected
        {
            get { return IsConnected; }
            
            set
            {
                if (value == IsConnected)
                    return;

                if (value == true )
                {
                    // connect to the device
                    try
                    {  
                        myDome.IsConnected = DomeImpl.ConnectionState.CONNECTED;
                        if ( myDome.IsConnected != DomeImpl.ConnectionState.CONNECTED)
                        {
                            DomeImpl.logger.LogMessage(this.GetType().ToString(), "Failure to connect in DomeImpl.");
                        }
                    }
                    catch( System.Exception ex )        
                    {
                        DomeImpl.logger.LogMessage( this.GetType().ToString(), "Failure to connect in DomeImpl" + ex.Message);
                        throw ex;
                    }
                }
                else
                {
                    // Disconnect from the device
                    //Check for outstanding slews etc - pause, wait or cancel ?
                    try
                    {
                        if ( myDome.domeState == DomeImpl.DomeStates.DOME_STOPPED )
                        {
                            myDome.IsConnected = DomeImpl.ConnectionState.NOT_CONNECTED;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        DomeImpl.logger.LogMessage(this.GetType().ToString(), "Failure to disconnect in DomeImpl" + ex.Message);
                        throw new ASCOM.DriverException("Failed to disconnect due to running operations", ex);
                    }
                    Properties.Settings.Default.Save();
                }
            }
        }

        public string Description
        {
            get { return driverDescription; }
        }

        public string DriverInfo
        {
            get { return "This is the driverInfo for the Skybadger Dome Driver v1.0"; }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get { return 2; }
        }

        #endregion

        #region private properties and methods
        // here are some useful properties and methods that can be used as required
        // to help with

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private bool IsConnected
        {
            get
            {
                // TODO check that the driver hardware connection exists and is connected to the hardware
                return myDome.IsConnected == DomeImpl.ConnectionState.CONNECTED;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                //Add function to Dome Hardware for status check on comms to validate and recover if necessary
                //Check comms port
                //read motor drive board version or battery voltage back as a check
                
                throw new ASCOM.NotConnectedException(message);
            }
        }
        #endregion

        public void AbortSlew()
        {
            myDome.syncCommand( DomeImpl.DomeCmds.ABORT, 0.0); 
        }

        public double Altitude
        {
            get {
                return this.Altitude; 
                }
            set { 
                myDome.syncCommand( DomeImpl.DomeCmds.SLEW_TO_ALTITUDE, value ); 
                }
        }

        public bool AtHome
        {
            get { 
                return System.Math.Abs(myDome.azimuth - myDome.homePosition) <= DomeImpl.positioningPrecision; 
                }
        }

        public bool AtPark
        {
            get { 
                if( myDome.domeState == DomeImpl.DomeStates.PARKED && System.Math.Abs( myDome.parkAzimuth - myDome.azimuth ) < DomeImpl.positioningPrecision ) 
                    return true;
                else
                    return false; 
                }
            set { 
                  myDome.domeState = DomeImpl.DomeStates.PARKED;
                  myDome.parkAzimuth = (int) myDome.azimuth; 
                }
        }

        public double Azimuth
        {
            get {  return myDome.azimuth + myDome.azimuthSyncOffset; }
        }

        public bool CanFindHome
        {
            get { return false; }
        }

        public bool CanSetAltitude
        {
            get { return false; }
        }

        public bool CanSetAzimuth
        {
            get { return true; }
        }

        public bool CanPark
        {
            get { return true; }
        }

        public bool CanSetPark
        {
            get { return true; }
        }

        public bool CanSetShutter
        {
            get { return false; }
        }

        public bool CanSlave
        {
            get { return false; } //slaving is slaving in hardware - not software. 
        }

        public bool CanSyncAzimuth
        {
            get { return true; }
        }

        public void CloseShutter()
        {
            throw new System.NotImplementedException();
        }

        public void FindHome()
        {
            throw new System.NotImplementedException();
        }

        public string Name
        {
            get { return driverDescription; }
        }

        public void OpenShutter()
        {
            throw new System.NotImplementedException();
        }

        public void Park()
        {
            myDome.syncCommand(DomeImpl.DomeCmds.PARK, myDome.parkAzimuth );
        }

        public void SetPark()
        {
            myDome.parkAzimuth = (int) myDome.azimuth;

            //throw new System.NotImplementedException();
        }

        public ShutterState ShutterStatus
        {
            get { throw new System.NotImplementedException(); }
        }

        public bool Slaved
        {
            get
            {
                return false;
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public void SlewToAltitude(double Altitude)
        {
            throw new System.NotImplementedException();
        }

        public void SlewToAzimuth(double Azimuth)
        {
            myDome.syncCommand( DomeImpl.DomeCmds.SLEW_TO_AZIMUTH, Azimuth);            
        }

        public bool Slewing
        {
            get {
                bool output = (myDome.domeState == DomeImpl.DomeStates.SLEW_CCW || 
                               myDome.domeState == DomeImpl.DomeStates.SLEW_CCW_TIMED || 
                               myDome.domeState == DomeImpl.DomeStates.SLEW_CW || 
                               myDome.domeState == DomeImpl.DomeStates.SLEW_CW_TIMED || 
                               myDome.shutterState == DomeImpl.ShutterStates.SHUTTER_CLOSING || 
                               myDome.shutterState == DomeImpl.ShutterStates.SHUTTER_OPENING);
                return output;
                }
        }

        public void SyncToAzimuth(double Azimuth)
        {
            if (!Slewing)
            {
                myDome.azimuthSyncOffset = (int) ( Azimuth - myDome.azimuth);
                //Do some logging. 
            }
            else
            {
                //Log why ignored
                DomeImpl.logger.LogMessage(this.GetType().ToString(), "SyncToAzimuth:: Currently slewing so sync not accepted");
                throw new ASCOM.DriverException("SyncToAzimuth::Currently slewing so sync not accepted");
            }
        }

    }
}