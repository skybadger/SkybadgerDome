namespace ASCOM.Skybadger
{
    partial class SetupDialogForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.picASCOM = new System.Windows.Forms.PictureBox();
            this.labelObboI2CLCD = new System.Windows.Forms.Label();
            this.I2CLCDAddr = new System.Windows.Forms.TextBox();
            this.labelObboI2CMotor = new System.Windows.Forms.Label();
            this.I2CMotorCtrlAddr = new System.Windows.Forms.TextBox();
            this.labelObboSerial = new System.Windows.Forms.Label();
            this.groupDome = new System.Windows.Forms.GroupBox();
            this.DomeSerialPort = new System.Windows.Forms.ComboBox();
            this.labelDomeSerial = new System.Windows.Forms.Label();
            this.labelDomeI2CProxy = new System.Windows.Forms.Label();
            this.labelDomeI2CVolts = new System.Windows.Forms.Label();
            this.labelDomeI2CMag = new System.Windows.Forms.Label();
            this.I2CVoltsAddr = new System.Windows.Forms.TextBox();
            this.DomeI2CProxyAddr = new System.Windows.Forms.TextBox();
            this.I2CMagnetometerAddr = new System.Windows.Forms.TextBox();
            this.groupFixed = new System.Windows.Forms.GroupBox();
            this.ObboSerialPort = new System.Windows.Forms.ComboBox();
            this.ObboI2CProxyAddress = new System.Windows.Forms.Label();
            this.ObboI2CProxyAddr = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).BeginInit();
            this.groupDome.SuspendLayout();
            this.groupFixed.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Location = new System.Drawing.Point(103, 218);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(58, 24);
            this.cmdOK.TabIndex = 4;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(406, 217);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(58, 25);
            this.cmdCancel.TabIndex = 5;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 31);
            this.label1.TabIndex = 0;
            this.label1.Text = "Skybadger ASCOM Dome driver";
            // 
            // picASCOM
            // 
            this.picASCOM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picASCOM.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picASCOM.Image = global::ASCOM.Skybadger.Properties.Resources.ASCOM;
            this.picASCOM.Location = new System.Drawing.Point(513, 9);
            this.picASCOM.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.picASCOM.Name = "picASCOM";
            this.picASCOM.Size = new System.Drawing.Size(48, 56);
            this.picASCOM.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picASCOM.TabIndex = 3;
            this.picASCOM.TabStop = false;
            this.picASCOM.Click += new System.EventHandler(this.BrowseToAscom);
            this.picASCOM.DoubleClick += new System.EventHandler(this.BrowseToAscom);
            // 
            // labelObboI2CLCD
            // 
            this.labelObboI2CLCD.AutoSize = true;
            this.labelObboI2CLCD.Location = new System.Drawing.Point(9, 75);
            this.labelObboI2CLCD.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelObboI2CLCD.Name = "labelObboI2CLCD";
            this.labelObboI2CLCD.Size = new System.Drawing.Size(88, 13);
            this.labelObboI2CLCD.TabIndex = 0;
            this.labelObboI2CLCD.Text = "LCD I2C Address";
            // 
            // I2CLCDAddr
            // 
            this.I2CLCDAddr.Location = new System.Drawing.Point(163, 72);
            this.I2CLCDAddr.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.I2CLCDAddr.Name = "I2CLCDAddr";
            this.I2CLCDAddr.Size = new System.Drawing.Size(42, 20);
            this.I2CLCDAddr.TabIndex = 3;
            // 
            // labelObboI2CMotor
            // 
            this.labelObboI2CMotor.AutoSize = true;
            this.labelObboI2CMotor.Location = new System.Drawing.Point(9, 99);
            this.labelObboI2CMotor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelObboI2CMotor.Name = "labelObboI2CMotor";
            this.labelObboI2CMotor.Size = new System.Drawing.Size(141, 13);
            this.labelObboI2CMotor.TabIndex = 0;
            this.labelObboI2CMotor.Text = "Motor Controller I2C Address";
            // 
            // I2CMotorCtrlAddr
            // 
            this.I2CMotorCtrlAddr.Location = new System.Drawing.Point(163, 96);
            this.I2CMotorCtrlAddr.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.I2CMotorCtrlAddr.Name = "I2CMotorCtrlAddr";
            this.I2CMotorCtrlAddr.Size = new System.Drawing.Size(42, 20);
            this.I2CMotorCtrlAddr.TabIndex = 4;
            // 
            // labelObboSerial
            // 
            this.labelObboSerial.AutoSize = true;
            this.labelObboSerial.Location = new System.Drawing.Point(8, 27);
            this.labelObboSerial.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelObboSerial.Name = "labelObboSerial";
            this.labelObboSerial.Size = new System.Drawing.Size(91, 13);
            this.labelObboSerial.TabIndex = 0;
            this.labelObboSerial.Text = "Obbo Serial Port#";
            // 
            // groupDome
            // 
            this.groupDome.Controls.Add(this.DomeSerialPort);
            this.groupDome.Controls.Add(this.labelDomeSerial);
            this.groupDome.Controls.Add(this.labelDomeI2CProxy);
            this.groupDome.Controls.Add(this.labelDomeI2CVolts);
            this.groupDome.Controls.Add(this.labelDomeI2CMag);
            this.groupDome.Controls.Add(this.I2CVoltsAddr);
            this.groupDome.Controls.Add(this.DomeI2CProxyAddr);
            this.groupDome.Controls.Add(this.I2CMagnetometerAddr);
            this.groupDome.Location = new System.Drawing.Point(11, 70);
            this.groupDome.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupDome.Name = "groupDome";
            this.groupDome.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupDome.Size = new System.Drawing.Size(244, 139);
            this.groupDome.TabIndex = 1;
            this.groupDome.TabStop = false;
            this.groupDome.Text = "Dome fittings";
            // 
            // DomeSerialPort
            // 
            this.DomeSerialPort.AccessibleDescription = "This is the serial port used to communicate to the moving dome components";
            this.DomeSerialPort.FormattingEnabled = true;
            this.DomeSerialPort.Location = new System.Drawing.Point(163, 22);
            this.DomeSerialPort.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.DomeSerialPort.Name = "DomeSerialPort";
            this.DomeSerialPort.Size = new System.Drawing.Size(67, 21);
            this.DomeSerialPort.Sorted = true;
            this.DomeSerialPort.TabIndex = 0;
            // 
            // labelDomeSerial
            // 
            this.labelDomeSerial.AutoSize = true;
            this.labelDomeSerial.Location = new System.Drawing.Point(12, 29);
            this.labelDomeSerial.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDomeSerial.Name = "labelDomeSerial";
            this.labelDomeSerial.Size = new System.Drawing.Size(93, 13);
            this.labelDomeSerial.TabIndex = 0;
            this.labelDomeSerial.Text = "Dome Serial Port#";
            // 
            // labelDomeI2CProxy
            // 
            this.labelDomeI2CProxy.AutoSize = true;
            this.labelDomeI2CProxy.Location = new System.Drawing.Point(13, 53);
            this.labelDomeI2CProxy.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDomeI2CProxy.Name = "labelDomeI2CProxy";
            this.labelDomeI2CProxy.Size = new System.Drawing.Size(92, 13);
            this.labelDomeI2CProxy.TabIndex = 0;
            this.labelDomeI2CProxy.Text = "I2C Proxy address";
            // 
            // labelDomeI2CVolts
            // 
            this.labelDomeI2CVolts.AutoSize = true;
            this.labelDomeI2CVolts.Location = new System.Drawing.Point(13, 100);
            this.labelDomeI2CVolts.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDomeI2CVolts.Name = "labelDomeI2CVolts";
            this.labelDomeI2CVolts.Size = new System.Drawing.Size(128, 13);
            this.labelDomeI2CVolts.TabIndex = 0;
            this.labelDomeI2CVolts.Text = "Battery Volts I2C  address";
            // 
            // labelDomeI2CMag
            // 
            this.labelDomeI2CMag.AutoSize = true;
            this.labelDomeI2CMag.Location = new System.Drawing.Point(13, 77);
            this.labelDomeI2CMag.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDomeI2CMag.Name = "labelDomeI2CMag";
            this.labelDomeI2CMag.Size = new System.Drawing.Size(137, 13);
            this.labelDomeI2CMag.TabIndex = 0;
            this.labelDomeI2CMag.Text = "Magnetometer I2C  address";
            // 
            // I2CVoltsAddr
            // 
            this.I2CVoltsAddr.Location = new System.Drawing.Point(163, 95);
            this.I2CVoltsAddr.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.I2CVoltsAddr.Name = "I2CVoltsAddr";
            this.I2CVoltsAddr.Size = new System.Drawing.Size(43, 20);
            this.I2CVoltsAddr.TabIndex = 2;
            // 
            // DomeI2CProxyAddr
            // 
            this.DomeI2CProxyAddr.Location = new System.Drawing.Point(163, 49);
            this.DomeI2CProxyAddr.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.DomeI2CProxyAddr.Name = "DomeI2CProxyAddr";
            this.DomeI2CProxyAddr.Size = new System.Drawing.Size(43, 20);
            this.DomeI2CProxyAddr.TabIndex = 1;
            // 
            // I2CMagnetometerAddr
            // 
            this.I2CMagnetometerAddr.Location = new System.Drawing.Point(163, 72);
            this.I2CMagnetometerAddr.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.I2CMagnetometerAddr.Name = "I2CMagnetometerAddr";
            this.I2CMagnetometerAddr.Size = new System.Drawing.Size(43, 20);
            this.I2CMagnetometerAddr.TabIndex = 2;
            // 
            // groupFixed
            // 
            this.groupFixed.Controls.Add(this.ObboSerialPort);
            this.groupFixed.Controls.Add(this.ObboI2CProxyAddress);
            this.groupFixed.Controls.Add(this.I2CLCDAddr);
            this.groupFixed.Controls.Add(this.labelObboI2CMotor);
            this.groupFixed.Controls.Add(this.labelObboSerial);
            this.groupFixed.Controls.Add(this.I2CMotorCtrlAddr);
            this.groupFixed.Controls.Add(this.ObboI2CProxyAddr);
            this.groupFixed.Controls.Add(this.labelObboI2CLCD);
            this.groupFixed.Location = new System.Drawing.Point(272, 71);
            this.groupFixed.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupFixed.Name = "groupFixed";
            this.groupFixed.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupFixed.Size = new System.Drawing.Size(243, 138);
            this.groupFixed.TabIndex = 2;
            this.groupFixed.TabStop = false;
            this.groupFixed.Text = "Fixed fittings";
            // 
            // ObboSerialPort
            // 
            this.ObboSerialPort.FormattingEnabled = true;
            this.ObboSerialPort.Location = new System.Drawing.Point(162, 21);
            this.ObboSerialPort.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.ObboSerialPort.Name = "ObboSerialPort";
            this.ObboSerialPort.Size = new System.Drawing.Size(67, 21);
            this.ObboSerialPort.TabIndex = 1;
            // 
            // ObboI2CProxyAddress
            // 
            this.ObboI2CProxyAddress.AutoSize = true;
            this.ObboI2CProxyAddress.Location = new System.Drawing.Point(9, 51);
            this.ObboI2CProxyAddress.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ObboI2CProxyAddress.Name = "ObboI2CProxyAddress";
            this.ObboI2CProxyAddress.Size = new System.Drawing.Size(92, 13);
            this.ObboI2CProxyAddress.TabIndex = 0;
            this.ObboI2CProxyAddress.Text = "I2C Proxy address";
            // 
            // ObboI2CProxyAddr
            // 
            this.ObboI2CProxyAddr.Location = new System.Drawing.Point(162, 47);
            this.ObboI2CProxyAddr.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.ObboI2CProxyAddr.Name = "ObboI2CProxyAddr";
            this.ObboI2CProxyAddr.Size = new System.Drawing.Size(43, 20);
            this.ObboI2CProxyAddr.TabIndex = 2;
            // 
            // SetupDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(569, 254);
            this.Controls.Add(this.groupFixed);
            this.Controls.Add(this.groupDome);
            this.Controls.Add(this.picASCOM);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupDialogForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Skybadger Dome Setup";
            this.Load += new System.EventHandler(this.SetupDialogForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).EndInit();
            this.groupDome.ResumeLayout(false);
            this.groupDome.PerformLayout();
            this.groupFixed.ResumeLayout(false);
            this.groupFixed.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox picASCOM;
        private System.Windows.Forms.Label labelObboI2CLCD;
        private System.Windows.Forms.TextBox I2CLCDAddr;
        private System.Windows.Forms.Label labelObboI2CMotor;
        private System.Windows.Forms.TextBox I2CMotorCtrlAddr;
        private System.Windows.Forms.Label labelObboSerial;
        private System.Windows.Forms.GroupBox groupDome;
        private System.Windows.Forms.Label labelDomeSerial;
        private System.Windows.Forms.GroupBox groupFixed;
        private System.Windows.Forms.Label labelDomeI2CMag;
        private System.Windows.Forms.TextBox I2CMagnetometerAddr;
        private System.Windows.Forms.Label labelDomeI2CProxy;
        private System.Windows.Forms.TextBox DomeI2CProxyAddr;
        private System.Windows.Forms.ComboBox DomeSerialPort;
        private System.Windows.Forms.ComboBox ObboSerialPort;
        private System.Windows.Forms.Label labelDomeI2CVolts;
        private System.Windows.Forms.TextBox I2CVoltsAddr;
        private System.Windows.Forms.Label ObboI2CProxyAddress;
        private System.Windows.Forms.TextBox ObboI2CProxyAddr;
    }
}