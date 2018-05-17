namespace ASCOM.Skybadger
{
    partial class Form1
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
            this.buttonChoose = new System.Windows.Forms.Button();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonSetPark = new System.Windows.Forms.Button();
            this.buttonSetHome = new System.Windows.Forms.Button();
            this.buttonSyncAz = new System.Windows.Forms.Button();
            this.ParkLabel = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.parkText = new System.Windows.Forms.TextBox();
            this.homeText = new System.Windows.Forms.TextBox();
            this.syncOffsetText = new System.Windows.Forms.TextBox();
            this.buttonClose = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonChoose
            // 
            this.buttonChoose.Location = new System.Drawing.Point(9, 149);
            this.buttonChoose.Name = "buttonChoose";
            this.buttonChoose.Size = new System.Drawing.Size(72, 23);
            this.buttonChoose.TabIndex = 7;
            this.buttonChoose.Text = "Choose";
            this.buttonChoose.UseVisualStyleBackColor = true;
            this.buttonChoose.Click += new System.EventHandler(this.buttonChoose_Click);
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(101, 149);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(72, 23);
            this.buttonConnect.TabIndex = 8;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonSetPark);
            this.groupBox2.Controls.Add(this.buttonSetHome);
            this.groupBox2.Controls.Add(this.buttonSyncAz);
            this.groupBox2.Controls.Add(this.ParkLabel);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.parkText);
            this.groupBox2.Controls.Add(this.homeText);
            this.groupBox2.Controls.Add(this.syncOffsetText);
            this.groupBox2.Location = new System.Drawing.Point(11, 10);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox2.Size = new System.Drawing.Size(269, 123);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Toolbox";
            // 
            // buttonSetPark
            // 
            this.buttonSetPark.Location = new System.Drawing.Point(158, 82);
            this.buttonSetPark.Margin = new System.Windows.Forms.Padding(1);
            this.buttonSetPark.Name = "buttonSetPark";
            this.buttonSetPark.Size = new System.Drawing.Size(96, 20);
            this.buttonSetPark.TabIndex = 6;
            this.buttonSetPark.Text = "Set Park";
            this.buttonSetPark.UseVisualStyleBackColor = true;
            this.buttonSetPark.Click += new System.EventHandler(this.buttonSetPark_Click);
            // 
            // buttonSetHome
            // 
            this.buttonSetHome.Location = new System.Drawing.Point(158, 53);
            this.buttonSetHome.Margin = new System.Windows.Forms.Padding(1);
            this.buttonSetHome.Name = "buttonSetHome";
            this.buttonSetHome.Size = new System.Drawing.Size(96, 22);
            this.buttonSetHome.TabIndex = 4;
            this.buttonSetHome.Text = "Set Home";
            this.buttonSetHome.UseVisualStyleBackColor = true;
            this.buttonSetHome.Click += new System.EventHandler(this.buttonSetHome_Click);
            // 
            // buttonSyncAz
            // 
            this.buttonSyncAz.Location = new System.Drawing.Point(158, 26);
            this.buttonSyncAz.Margin = new System.Windows.Forms.Padding(1);
            this.buttonSyncAz.Name = "buttonSyncAz";
            this.buttonSyncAz.Size = new System.Drawing.Size(96, 20);
            this.buttonSyncAz.TabIndex = 2;
            this.buttonSyncAz.Text = "Sync Azimuth";
            this.buttonSyncAz.UseVisualStyleBackColor = true;
            this.buttonSyncAz.Click += new System.EventHandler(this.buttonSyncAz_Click_1);
            // 
            // ParkLabel
            // 
            this.ParkLabel.AutoSize = true;
            this.ParkLabel.Location = new System.Drawing.Point(15, 84);
            this.ParkLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ParkLabel.Name = "ParkLabel";
            this.ParkLabel.Size = new System.Drawing.Size(69, 13);
            this.ParkLabel.TabIndex = 2;
            this.ParkLabel.Text = "Park Position";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 58);
            this.label7.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(54, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Set Home";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(15, 31);
            this.label10.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(59, 13);
            this.label10.TabIndex = 2;
            this.label10.Text = "DomeSync";
            // 
            // parkText
            // 
            this.parkText.Location = new System.Drawing.Point(90, 82);
            this.parkText.Margin = new System.Windows.Forms.Padding(1);
            this.parkText.Name = "parkText";
            this.parkText.Size = new System.Drawing.Size(52, 20);
            this.parkText.TabIndex = 5;
            // 
            // homeText
            // 
            this.homeText.Location = new System.Drawing.Point(90, 55);
            this.homeText.Margin = new System.Windows.Forms.Padding(1);
            this.homeText.Name = "homeText";
            this.homeText.Size = new System.Drawing.Size(52, 20);
            this.homeText.TabIndex = 3;
            // 
            // syncOffsetText
            // 
            this.syncOffsetText.Location = new System.Drawing.Point(90, 26);
            this.syncOffsetText.Margin = new System.Windows.Forms.Padding(1);
            this.syncOffsetText.Name = "syncOffsetText";
            this.syncOffsetText.Size = new System.Drawing.Size(52, 20);
            this.syncOffsetText.TabIndex = 1;
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(190, 149);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 9;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click_1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(312, 187);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.buttonChoose);
            this.Name = "Form1";
            this.Text = "Skybadger Dome Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonChoose;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonSetPark;
        private System.Windows.Forms.Button buttonSetHome;
        private System.Windows.Forms.Button buttonSyncAz;
        private System.Windows.Forms.Label ParkLabel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox parkText;
        private System.Windows.Forms.TextBox homeText;
        private System.Windows.Forms.TextBox syncOffsetText;
        private System.Windows.Forms.Button buttonClose;
    }
}

