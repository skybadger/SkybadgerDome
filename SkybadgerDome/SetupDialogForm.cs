using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
//using ASCOM.Utilities.Serial;
namespace ASCOM.Skybadger
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        public SetupDialogForm()
        {
            InitializeComponent();
            
            //Rotating - dome
            DomeSerialPort.Text = Properties.Settings.Default.DomeCommPort;
            DomeI2CProxyAddr.Text = String.Format("{0}", Properties.Settings.Default.I2CDomeProxyAddr);
            I2CMagnetometerAddr.Text = String.Format("{0}", Properties.Settings.Default.I2CMagnetometerAddr);
            I2CVoltsAddr.Text = String.Format("{0}", Properties.Settings.Default.I2CVoltsAddr);
            
            //Fixed - Obbo
            ObboSerialPort.Text = Properties.Settings.Default.ObboCommPort;
            ObboI2CProxyAddr.Text = String.Format("{0}", Properties.Settings.Default.I2CObboProxyAddr);
            I2CMotorCtrlAddr.Text = String.Format("{0}", Properties.Settings.Default.I2CMotorCtrlAddr);
            I2CLCDAddr.Text = String.Format("{0}", Properties.Settings.Default.I2CLCDAddr);
            
            //Setup available comm ports in drop down.
            ASCOM.Utilities.Serial serial = new Utilities.Serial();

            if (serial.AvailableCOMPorts.Length == 0)
            {
                MessageBox.Show("Unable to find ANY serial ports. Two serial ports are required - one for the Dome and one for the fixed components. \n Click 'Cancel' and check your connections. ");
            }
            else if (serial.AvailableCOMPorts.Length == 1)
            {
                MessageBox.Show("Expect at least TWO serial ports. One for the dome and one for the fixed components.\n Click 'Cancel' and check your connections. ");
            }
            else
                foreach (var item in serial.AvailableCOMPorts)
            {
                String portName;
                portName = Properties.Settings.Default.ObboCommPort.ToUpper();
                ObboSerialPort.Items.Add(item);
                if (portName.Contains(item))
                    ObboSerialPort.SelectedIndex = ObboSerialPort.Items.IndexOf(item.ToUpper() );

                portName = Properties.Settings.Default.DomeCommPort.ToUpper();
                DomeSerialPort.Items.Add(item);
                if ( portName.Contains(item))
                    DomeSerialPort.SelectedIndex = DomeSerialPort.Items.IndexOf(item.ToUpper() );
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            Int16 i2cAddress = 0;
            String commName;

            //save the settings and update the cache...
            //Expecting names of the type "comN"            
            //Dome
            commName = ((String)( DomeSerialPort.SelectedItem)).ToUpper();
            if (commName.StartsWith("COM") && commName.Length > 3 && DomeSerialPort.SelectedIndex >= 0 )
                Properties.Settings.Default.DomeCommPort = ((String)(DomeSerialPort.SelectedItem)).ToUpper();

            i2cAddress = System.Int16.Parse(DomeI2CProxyAddr.Text);
            if (i2cAddress < 256 && i2cAddress >= 0)
                Properties.Settings.Default.I2CDomeProxyAddr = System.Int16.Parse( DomeI2CProxyAddr.Text);

            i2cAddress = System.Int16.Parse(I2CMagnetometerAddr.Text);
            if (i2cAddress < 256 && i2cAddress >= 0)
                Properties.Settings.Default.I2CMagnetometerAddr = System.Int16.Parse(I2CMagnetometerAddr.Text);

            i2cAddress = System.Int16.Parse(I2CVoltsAddr.Text);
            if (i2cAddress < 256 && i2cAddress >= 0)
                Properties.Settings.Default.I2CVoltsAddr = System.Int16.Parse(I2CVoltsAddr.Text);

            //Fixed
            commName = ((String)(ObboSerialPort.SelectedItem)).ToUpper();
            if (commName.StartsWith("COM") && commName.Length > 3 && ObboSerialPort.SelectedIndex >= 0 )
                Properties.Settings.Default.ObboCommPort = ((String)(ObboSerialPort.SelectedItem)).ToUpper();

            i2cAddress = System.Int16.Parse(ObboI2CProxyAddr.Text);
            if (i2cAddress < 256 && i2cAddress >= 0)
                Properties.Settings.Default.I2CObboProxyAddr = System.Int16.Parse(DomeI2CProxyAddr.Text);

            i2cAddress = System.Int16.Parse(I2CLCDAddr.Text);
            if (i2cAddress < 256 && i2cAddress >= 0)
                Properties.Settings.Default.I2CLCDAddr = System.Int16.Parse(I2CLCDAddr.Text);                                    
            
            i2cAddress = System.Int16.Parse(I2CMotorCtrlAddr.Text);
            if (i2cAddress < 256 && i2cAddress >= 0)
                Properties.Settings.Default.I2CMotorCtrlAddr = System.Int16.Parse(I2CMotorCtrlAddr.Text);

            //Properties.Settings.Default.Save(); do this in form handler.
            
            //Doesn't seem to be specified anywhere else.
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            //Don't save settings, just close;
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        private void BrowseToAscom(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void SetupDialogForm_Load(object sender, EventArgs e)
        {

        }

    }
}