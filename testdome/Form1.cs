using System;
using System.Windows.Forms;

namespace ASCOM.Skybadger
{
    public partial class Form1 : Form
    {
/* Header comments
 */
        private ASCOM.DriverAccess.Dome driver;

        public Form1()
        {
            InitializeComponent();
            SetUIState();
            syncOffsetText.Text = "0";
            homeText.Text = "180";
            parkText.Text = "180";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsConnected)
            { 
                if( driver.Slewing)
                    driver.AbortSlew();                    
                driver.Connected = false;
            }
            //Properties.Settings.Default.Save();
            //Don't save settings, just close;
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            //Close();            
        }

        private void buttonChoose_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DriverId = ASCOM.DriverAccess.Dome.Choose(Properties.Settings.Default.DriverId);
            SetUIState();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.Connected = false;
                driver = null;
            }
            else 
            {
                //don't create new when re-clicking existing
                if (driver == null) 
                    driver = new ASCOM.DriverAccess.Dome(Properties.Settings.Default.DriverId);
                //try to make a connection
                driver.Connected = true;
                if ( !driver.Connected )
                {
                    MessageBox.Show("Unable to connect - check logs", "Connection error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            SetUIState();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                if (driver.Slewing)
                    driver.AbortSlew();
                driver.Connected = false;
            }
            //Properties.Settings.Default.Save();
            //Don't save settings, just close;
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Application.Exit();
        }

        private void SetUIState()
        {
            buttonConnect.Enabled = !string.IsNullOrEmpty(Properties.Settings.Default.DriverId);
            buttonChoose.Enabled = !IsConnected;
            buttonConnect.Text = IsConnected ? "Disconnect" : "Connect";
        }

        private bool IsConnected
        {
            get
            {
                return ((this.driver != null) && (driver.Connected == true));
            }
        }

        // Update dome driver with current position
        private void buttonSyncAz_Click_1(object sender, EventArgs e)
        {
            if (IsConnected && !string.IsNullOrEmpty( syncOffsetText.Text)) 
                driver.SyncToAzimuth( (double) System.Double.Parse( syncOffsetText.Text));
        }

        //Update dome driver with desired position of home. 
        //Currently dome interface doesn't support telling dome where its home should be.. Dyuh
        private void buttonSetHome_Click(object sender, EventArgs e)
        {
            int homeAzimuth = 0;          
            if (!string.IsNullOrEmpty(parkText.Text) && IsConnected)           
                try
                {                    
                    int.TryParse( homeText.Text, out homeAzimuth);
                    homeAzimuth = homeAzimuth % 360;
                    /* driver.SlewToAzimuth(parkAzimuth);
                    driver.SetPark();
                    driver.SlewToAzimuth(orgAzimuth);
                    */
                }
                catch (SystemException Ex)
                {
                    //handle format errors
                    //put up as slewing message CheckBox to explain length ArgumentOutOfRangeException time and provide means of cancelling;
                }
        }

        //tell the dome where its park position is
        //the dome interface requires the dome to be moved to the park position to achieve this. 
        private void buttonSetPark_Click(object sender, EventArgs e)
        {
        int parkAzimuth = 0;
        int orgAzimuth;
        if (!string.IsNullOrEmpty(parkText.Text) && IsConnected)
            try
            {
                orgAzimuth = (short)driver.Azimuth;
                int.TryParse(parkText.Text, out parkAzimuth);
                parkAzimuth = parkAzimuth % 360;
                driver.SlewToAzimuth(parkAzimuth);
                driver.SetPark();
                driver.SlewToAzimuth(orgAzimuth);
            }
            catch (SystemException Ex) 
            {
                //handle format errors
                //put up as slewing message CheckBox to explain length ArgumentOutOfRangeException time and provide means of cancelling;
            }
        }

        private void buttonClose_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
