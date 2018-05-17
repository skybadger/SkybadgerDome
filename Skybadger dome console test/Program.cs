//#define TEST1_CONNECTIVITY
//#define TEST2_MOTOR_CONTROL
#define TEST3_ASYNC_MOTOR

// This implements a console application that can be used to test an ASCOM driver
//

// This is used to define code in the template that is specific to one class implementation
// unused code can be deleted and this definition removed.

#define Dome
// remove this to bypass the code that uses the chooser to select the driver
#define UseChooser

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ASCOM
{
    class Program
    {
        static ASCOM.DriverAccess.Dome device; 
        static void Main(string[] args)
        {
            // choose the device
            int timedout = 0;
            //turn the dome by time, not bearing
            DateTime dt = new System.DateTime();
            ASCOM.DriverAccess.Dome device;
            
            // Uncomment the code that's required
#if UseChooser
            string id = ASCOM.DriverAccess.Dome.Choose("");
            if (string.IsNullOrEmpty(id))
                return;
            try
            {
                // create this device
                device = new ASCOM.DriverAccess.Dome(id);
#else
            // this can be replaced by this code, it avoids the chooser and creates the driver class directly.
            try
            {
                device = new ASCOM.DriverAccess.Dome("ASCOM.Skybadger.Dome");
#endif
                
                // now run some tests, adding code to your driver so that the tests will pass.
                // these first tests are common to all drivers.
                Console.WriteLine("name " + device.Name);
                Console.WriteLine("description " + device.Description);
                Console.WriteLine("DriverInfo " + device.DriverInfo);
                Console.WriteLine("driverVersion " + device.DriverVersion);

                System.Threading.Thread.Sleep(500);

#if TEST1_CONNECTIVITY
                // TODO add more code to test the driver.
                Console.WriteLine("Setting 'Connected' to 'True'");
                device.Connected = true;

                if (device.Connected)
                {
                    String output;
                    Console.WriteLine(" 'CanFindHome' set to: " + device.CanFindHome);
                    Console.WriteLine(" 'CanSetPark' set to: " + device.CanSetPark);
                    Console.WriteLine(" 'CanPark' set to: " + device.CanPark);
                    Console.WriteLine(" 'CanSetAltitude' set to: " + device.CanSetAltitude);
                    Console.WriteLine(" 'CanSetAzimuth' set to: " + device.CanSetAzimuth);
                    Console.WriteLine(" 'CanSetPark' set to: " + device.CanSetPark);
                    Console.WriteLine(" 'CanSetShutter' set to: " + device.CanSetShutter);
                    Console.WriteLine(" 'CanSlave' set to: " + device.CanSlave);
                    Console.WriteLine(" 'CanSyncAzimuth' set to: " + device.CanSyncAzimuth);

                    Console.WriteLine("Press Enter to continue");
                    Console.ReadLine();

                    Console.WriteLine("Setting 'Connected' to 'False'");
                }

                device.Connected = false;
            }
            catch (ASCOM.DriverException dex)
            {
                Console.WriteLine(" ASCOM exception : " + dex.Message);
                if (dex.InnerException != null)
                    Console.WriteLine(" +inner exception : " + dex.InnerException);
            }
#endif
#if TEST2_MOTOR_CONTROL                   
            try
            {
                    dt = System.DateTime.Now.AddSeconds(20);
                    Console.WriteLine(String.Format("Timeout set to {0}", dt));

                    output = device.CommandString("rotateRight", false); 
                    Console.WriteLine(" rotateRight requested: " + output);
                    do
                    {
                        output = device.CommandString("Bearing", false);
                        Console.WriteLine(" Bearing: " + output);
                        output = device.CommandString("Voltage", false);
                        Console.WriteLine(" Voltage: " + output);
                        
                        System.Threading.Thread.Sleep(1000);
                        Console.WriteLine(String.Format("Time: {0}", System.DateTime.Now ));
                        timedout= DateTime.Compare(dt, System.DateTime.Now);
                    } while ( timedout > 0 );
                    if ( device.Slewing) 
                        device.AbortSlew();
                    
                    Console.WriteLine("Press Enter to continue");
                    Console.ReadLine();

                    dt = System.DateTime.Now.AddSeconds(20);
                    Console.WriteLine(String.Format("Timeout set to {0}", dt));

                    output = device.CommandString("rotateRight", false);
                    Console.WriteLine(" RotateRight requested " + output);
                    do
                    {
                        
                        output = device.CommandString("Bearing", false);
                        Console.WriteLine(" Bearing: " + output );
                        output = device.CommandString("Voltage", false);
                        Console.WriteLine(" Voltage: " + output);
                        
                        System.Threading.Thread.Sleep(1000);
                        Console.WriteLine(String.Format("Time: {0}", System.DateTime.Now));
                        timedout = DateTime.Compare(dt, System.DateTime.Now);
                    } while (timedout > 0);
                    if (device.Slewing)
                        device.AbortSlew();
                }
            }
            catch( ASCOM.DriverException dex )
            {
                Console.WriteLine(" ASCOM exception : " + dex.Message);
                if ( dex.InnerException != null ) 
                    Console.WriteLine ( " +inner exception : " + dex.InnerException);
            }
#endif
#if TEST3_ASYNC_MOTOR
                String output;
                
                device = new ASCOM.DriverAccess.Dome("ASCOM.Skybadger.Dome");
                device.Connected = true; 
                //Console.WriteLine(String.Format("Slewing to : {0}", 180));
                device.SlewToAzimuth(180);
                do {
                        output = device.CommandString("Bearing", false);
                        Console.WriteLine(" Bearing: " + output );
                        System.Threading.Thread.Sleep(1000);
                }while( device.Slewing );
                Console.WriteLine("Press Enter to SetPark");
                Console.ReadLine();
                device.SetPark();                
               
                Console.WriteLine(String.Format("Slewing to : {0}", 90));
                device.SlewToAzimuth(90);
                do {
                        output = device.CommandString("Bearing", false);
                        Console.WriteLine(" Bearing: " + output );
                        System.Threading.Thread.Sleep(1000);
                }while( device.Slewing );

                
                Console.WriteLine(String.Format("Slewing to : {0}", 270));
                device.SlewToAzimuth(270);
                do {
                        output = device.CommandString("Bearing", false);
                        Console.WriteLine(" Bearing: " + output );
                        System.Threading.Thread.Sleep(1000);
                }while( device.Slewing );

                /*
                Console.WriteLine(String.Format("Slewing to : {0}", 180));
                device.SlewToAzimuth(180);
                do {
                        output = device.CommandString("Bearing", false);
                        Console.WriteLine(" Bearing: " + output );
                        System.Threading.Thread.Sleep(1000);
                }while( device.Slewing );
                Console.WriteLine(String.Format("Slewing to : {0}", 180));
                do {
                        output = device.CommandString("Bearing", false);
                        Console.WriteLine(" Bearing: " + output );                
                }while( device.Slewing );
                */
                if ( device.CanPark)
                    device.Park();

                Console.WriteLine("Press Enter to disconnect");
                Console.ReadLine();

                device.Connected = false; 
                device = null;

                Console.WriteLine("Disconnected");
            }
            catch( ASCOM.DriverException dex )
            {
                Console.WriteLine(" ASCOM exception : " + dex.Message);
                if ( dex.InnerException != null ) 
                    Console.WriteLine ( " +inner exception : " + dex.InnerException);
            }

#endif
            Console.WriteLine("Press Enter to finish");
            Console.ReadLine();
        }
    }
}
