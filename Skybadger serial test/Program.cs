//#define LCD
//#define MAGNETOMETER
#define MAGNETOMETER_58883L

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASCOM.Utilities;
using ASCOM;
using System.Threading;

namespace Skybadger_serial_test
{
    class Program
    {
        static void Main(string[] args)
        {
            ASCOM.Utilities.Serial comPort = new Serial();

            byte[] outByte;
            byte proxyAddress = 0x55;
            byte I2CDeviceAddress = 0xc6;
            String response;
            bool validResponse = false;
            bool quitLoop = false;
            byte dataCount = 0;
            String[] portlist = comPort.AvailableCOMPorts;
            int row = 0;
            do
            {  
                Console.WriteLine("Select comPort ID (preserve case!) from list below");
                for (int i = 0; i < portlist.Length; i++)
                    Console.WriteLine(portlist[i]);

                response = Console.ReadLine();
                if (response == "Q" || response == "q")
                {
                    quitLoop = true;
                    continue;
                }
                else if ( String.IsNullOrEmpty( response ) )
                    continue;

                for (int i = 0; i < portlist.Length; i++)
                {
                    if (portlist[i].CompareTo(response) == 0)
                    {
                        validResponse = true;
                    }
                }

                if (validResponse)
                {
                    //Setup comport settings
                    comPort.PortName = response;
                    comPort.Parity = SerialParity.None;
                    comPort.Handshake = SerialHandshake.None;
                    comPort.DTREnable = false;
                    comPort.DataBits = 8;
                    comPort.StopBits = SerialStopBits.One;
                    comPort.Speed = SerialSpeed.ps19200;
                    comPort.ReceiveTimeoutMs = 250;

                    try
                    {
#if LCD //Tested working without waits.

                        DateTime dt = DateTime.Now;
                        String timestamp = "UT:" + dt.ToLongTimeString();
                        char[] charString;
                        comPort.Connected = true;
                        byte datum = 0;
                        I2CDeviceAddress = 0xc6;

                        //send command - CLS
                        outByte = new byte[5];
                        outByte[0] = proxyAddress;                        //The proxy is a serial to I2C device
                        outByte[1] = (byte)((I2CDeviceAddress & 0xFE));   //I2C write clears LSB
                        outByte[2] = (byte)0;                             // device register to write
                        outByte[3] = 1;                                   //Length of data to write
                        outByte[4] = (byte)12;                            //Value to write

                        comPort.TransmitBinary(outByte);
                        //Thread.Sleep(150);

                        //Read response from proxy 
                        dataCount = comPort.ReceiveByte();
                        if (dataCount > 0)
                        {
                            Console.WriteLine("CLS operation:" + ((dataCount >0) ? "successful" : "failed"));
                        }

                        //send command - Move to row3, col5
                        outByte = new byte[7];
                        outByte[0] = proxyAddress;                        //The proxy is a serial to I2C device
                        outByte[1] = (byte)((I2CDeviceAddress & 0xFE));   //I2C write clears LSB
                        outByte[2] = (byte)0;                             //LCD write register is always 0
                        outByte[3] = 3;                                   //Length
                        outByte[4] = (byte)3;
                        outByte[5] = (byte)(row++%4);
                        outByte[6] = (byte)3;

                        comPort.TransmitBinary(outByte);
                        //Thread.Sleep(150);

                        //Read response from proxy 
                        dataCount = comPort.ReceiveByte();
                        if (dataCount > 0)
                        {
                            Console.WriteLine("Cursor set operation:" + ((dataCount > 0) ? "successful" : "failed"));
                        }

                        //send command - Set backlight on
                        outByte = new byte[5];
                        outByte[0] = proxyAddress;                        //The proxy is a serial to I2C device
                        outByte[1] = (byte)((I2CDeviceAddress & 0xFE));   //I2C write clears LSB
                        outByte[2] = (byte) 0;                            // device register to write
                        outByte[3] = 1;                                  //LCD write register is always 0
                        outByte[4] = 19;
                        
                        comPort.TransmitBinary(outByte);
                        //Thread.Sleep(150);

                        //Read response from proxy 
                        dataCount = comPort.ReceiveByte();
                        if (dataCount > 0)
                        {
                            Console.WriteLine("Backlight operation:" + ((dataCount > 0) ? "successful" : "failed"));
                        }

                        outByte = new byte[4 + timestamp.Length];
                        outByte[0] = (byte) (proxyAddress);                        //The proxy is a serial to I2C device
                        outByte[1] = (byte)((I2CDeviceAddress & 0xFE));            //I2C write clears LSB
                        outByte[2] = (byte)0;
                        outByte[3] = (byte)timestamp.Length;                       //length of data to write

                        //Write string to device display
                        charString = timestamp.ToCharArray();
                        for (int i = 0; i < charString.Length; i++)
                        {
                            outByte[i+4] = (byte)charString[i];
                        }

                        comPort.TransmitBinary(outByte);
                        Thread.Sleep(150);

                        //Read response from proxy 
                        datum = comPort.ReceiveByte();
                        Console.WriteLine("LCD Write:" + ((datum >0)?"Successful":"Failed"));
                        
#endif
#if MAGNETOMETER_DEVASYS
                        I2CDeviceAddress = 0xc0;
                        comPort.Connected = true;
                        
                        //send command
                        outByte = new byte[4];
                        outByte[0] = proxyAddress;                        //The proxy is a serial to I2C device
                        outByte[1] = (byte)(I2CDeviceAddress | 0x01);     //I2C read needs LSB set
                        outByte[2] = (byte) 0;                            // device register to read
                        outByte[3] = 2;                                   //length of data to read
                        comPort.TransmitBinary(outByte);
                        Thread.Sleep(50);

                        //Read response from proxy 
                        inByte = comPort.ReceiveCountedBinary(2);
                        if (inByte.Length == 2)
                        {
                            Console.WriteLine("Received data is version number:" + inByte[0] + " & bearing:" + inByte[1]);
                        }
                        
                        //send command for higher-res bearing
                        outByte[0] = proxyAddress;                        //The proxy is a serial to I2C device
                        outByte[1] = (byte)(I2CDeviceAddress | 0x01);     //I2C read needs LSB set
                        outByte[2] = (byte)2;                             //Device register to read
                        outByte[3] = 2;                                   //length of data to read
                        comPort.TransmitBinary(outByte);
                        Thread.Sleep(50);

                        //Read response from proxy 
                        inByte = comPort.ReceiveCountedBinary(2);
                        if (inByte.Length > 0 )
                        {
                            Console.WriteLine("Received data is bearing of " + (inByte[1] + (inByte[0] << 8)) / 10.00 + " Degrees");
                        }
                        outByte = new byte[4];
#endif
#if MAGNETOMETER_58883L
                        I2CDeviceAddress = 0x3d;
                        comPort.Connected = true;
                        byte[] inByte;
                        
                        //Need to enable magnetometer 
                        //send command - 0 to msb of zero register.
                        outByte = new byte[5];
                        outByte[0] = proxyAddress;                        //The proxy is a serial to I2C device
                        outByte[1] = (byte)(I2CDeviceAddress &0xFE );     //I2C write needs LSB clear
                        outByte[2] = (byte) 01;                            // device register to write
                        outByte[3] = 1;                                   //length of data to write
                        outByte[4] = 0xF0;
                        
                        comPort.TransmitBinary(outByte);
                        //Thread.Sleep(200);

                        //Read response from proxy 
                        dataCount = comPort.ReceiveByte();
                        if ( dataCount > 0 )
                        {
                            Console.WriteLine("Wrote config byte successfully");
                        }

                        //and set the gain
                        outByte = new byte[5];
                        outByte[0] = proxyAddress;                        //The proxy is a serial to I2C device
                        outByte[1] = (byte)(I2CDeviceAddress & 0xFE);     //I2C write needs LSB clear
                        outByte[2] = (byte)0;                            // device register to write
                        outByte[3] = 1;                                   //length of data to write
                        outByte[4] = 0x40;

                        comPort.TransmitBinary(outByte);
                        //Thread.Sleep(200);

                        //Read response from proxy 
                        dataCount = comPort.ReceiveByte();
                        if (dataCount > 0)
                        {
                            Console.WriteLine("Wrote config byte successfully");
                        }
                        //send command mode->continuous sensing
                        outByte[0] = proxyAddress;                        //The proxy is a serial to I2C device
                        outByte[1] = (byte)(I2CDeviceAddress & 0xFE);     //I2C write needs LSB clear
                        outByte[2] = (byte)2;                             //Device register to read
                        outByte[3] = 1;                                   //length of data to read
                        outByte[4] = 0x00;
                        comPort.TransmitBinary(outByte);
                        //Thread.Sleep(200);

                        //Read response from proxy 
                        dataCount = comPort.ReceiveByte();
                        if ( dataCount > 0 )
                        {
                            Console.WriteLine("Wrote mode byte succesfully");
                        }

                        do
                        {
                            outByte = new byte[4];
                            //send command mode->continuous sensing
                            outByte[0] = proxyAddress;                        //The proxy is a serial to I2C device
                            outByte[1] = (byte)(I2CDeviceAddress | 0x01);     //I2C read needs LSB set
                            outByte[2] = (byte)3;                             //Device register to read
                            outByte[3] = 6;                                   //length of data to read
                            comPort.TransmitBinary(outByte);
                            //Thread.Sleep(200);

                            //Read response from proxy 
                            inByte = comPort.ReceiveCountedBinary(6);
                            if (dataCount > 0)
                            {
                                int x, y, z;
                                float bearing;
                                //Seems c# can't handle conversion of unsigned two-complement bytes to signed ints properly  - or I can't.
                                x = (inByte[0] << 8) | inByte[1];
                                x = ((inByte[0] & 0x80) != 0) ? x - 65536 : x;
                                y = (inByte[4] << 8) | inByte[5];
                                y = ((inByte[4] & 0x80) != 0) ? y - 65536 : y;
                                z = (inByte[2] << 8) | inByte[3];
                                z = ((inByte[2] & 0x80) != 0) ? z - 65536 : z;

                                //Guidance from Honeywell datasheet for bearing calculations. 
                                //https://aerocontent.honeywell.com/aero/common/documents/myaerospacecatalog-documents/Defense_Brochures-documents/Magnetic__Literature_Application_notes-documents/AN203_Compass_Heading_Using_Magnetometers.pdf
                                /*
                                 * bearing = 0.0F;
                                if (y > 0)
                                    bearing = (float)(90 - (Math.Atan(x/y) * 180 / Math.PI));
                                else if (y < 0)
                                    bearing = (float)(270 - (Math.Atan(x/y) * 180 / Math.PI));
                                else if (y == 0 && x < 0)
                                    bearing = (float)180.0F;
                                else if (y == 0 && x > 0)
                                    bearing = (float)0.0F;
                                */
                                //Direction (y<0) = 270 - [arcTAN(x/y)]*180/¹
                                //Direction (y=0, x<0) = 180.0
                                //Direction (y=0, x>0) = 0.0
                                //Original calc prior to datasheet advice
                                //aTAN2 returns a bearing where 0 lies along the x axis.
                                bearing = (float) ((180.0/Math.PI)* Math.Atan2( y, x ));
                                bearing = ( bearing + 360 ) % 360;

                                Console.WriteLine("Read direction registers successful");
                                
                                Console.WriteLine(" X x[1]x[0] {0:x} {1:x} {2:D4}", inByte[0], inByte[1], x);
                                Console.WriteLine(" Y y[1]y[0] {0:x} {1:x} {2:D4}", inByte[4], inByte[5], y);
                                Console.WriteLine(" Z z[1]z[0] {0:x} {1:x} {2:D4}", inByte[2], inByte[3], z);
                                Console.WriteLine("Bearing: {0:f}", bearing);
                                /*
                                int xmax = 450;
                                int xmin = -48;
                                int ymin= -531;
                                int ymax = 275;
                                double dx, dy;
                                dy = (ymax + ymin)/2.0;
                                dx = (xmax+xmin)/2.0;
                                
                                bearing = (float)((180.0 / Math.PI) * Math.Atan2( dy-y, dx-x ));
                                bearing = (bearing +360) % 360;
                                Console.WriteLine("Adjusted Bearing: {0:f}", bearing);
                                 * */
                            }
                            Thread.Sleep(1000);
                        } while (true);                        
#endif
                    }
                    catch (System.Exception sEx)
                    {
                        Console.WriteLine("Exception caught: " + sEx.Message);
                        if( sEx.InnerException != null )
                                if (!String.IsNullOrEmpty(sEx.InnerException.Message))
                                    Console.WriteLine("The inner exception was: " + sEx.InnerException.Message);
                        comPort.ClearBuffers();
                    }
//                    catch (ASCOM.Utilities.Exceptions.SerialPortInUseException sEx)
//                   catch (ASCOM.NotConnectedException ex) //Cant find the reference to use for this..
                    finally
                    {
                        comPort.Connected = false;
                    }
                }
            } while (!quitLoop);

            comPort.Connected = false;
        }
    }
    class SerTest
    {
    }

}
