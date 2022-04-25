using TestApllication.network;

namespace TestApllication
{
	partial class MainForm
	{
        public Session session;
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
            this.loginItem1 = new TestApllication.layout.LoginItem();
            this.licenseItem1 = new TestApllication.layout.LicenseItem();
            this.SuspendLayout();
            // 
            // loginItem1
            // 
            this.loginItem1.Location = new System.Drawing.Point(0, 0);
            this.loginItem1.Name = "loginItem1";
            this.loginItem1.Size = new System.Drawing.Size(621, 357);
            this.loginItem1.TabIndex = 0;
            this.loginItem1.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            this.loginItem1.btnCreateAcc.Click += new System.EventHandler(this.btnCreateAcc_Click);
            // 
            // licenseItem1
            // 
            this.licenseItem1.Location = new System.Drawing.Point(0, 0);
            this.licenseItem1.Name = "licenseItem1";
            this.licenseItem1.Size = new System.Drawing.Size(621, 357);
            this.licenseItem1.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 353);
            this.Controls.Add(this.licenseItem1);
            this.Controls.Add(this.loginItem1);
            this.Name = "MainForm";
            this.Text = "Main";
            this.ResumeLayout(false);

		}

        #endregion

        public layout.LoginItem loginItem1;
        public layout.LicenseItem licenseItem1;
    }
}

