using System;
using TestApllication.Properties;
using TestApllication.util;
using TestApllication.network;
using TestApllication.DAO;

namespace TestApllication.layout
{
	partial class LicenseItem
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Getter-Setter
		public Session Session
		{
			get
			{
				return session;
			}
			set
			{
				session = new Session(value);
				if (session.IsLocked)
                {
					txtLicenseInp.Text = Resources.MSG_LIC_INFO_003;
					pnBuy.Visible = false;
				}
				else if (session.IsExpired)
				{
					txtLicenseInp.Text = Resources.MSG_LIC_INFO_002;
					pnBuy.Visible = false;
				}
				else
				{
					txtLicenseInp.Text = string.Format(Resources.MSG_LIC_INFO_001, DateTimeUtil.GetDaysRemain(DateTime.Today, session.UsedEndDate));
					pnBuy.Visible = true;
				}
			}
		}

		public string LicenseTypeChoose
		{
			get
			{
				LicenseTypeItemDAO item = (LicenseTypeItemDAO) drpLicenseType.SelectedItem;
				return item.LicenseTypeId;
			}
		}

		public string LicenseKeyInput
		{
			get
			{
				return txtLicenseInp.Text ?? string.Empty;
			}
		}
		#endregion

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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lblLicenseStatus = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.txtLicenseInp = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnBuy = new System.Windows.Forms.Button();
			this.drpLicenseType = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.pnBuy = new System.Windows.Forms.Panel();
			this.btnInputLicense = new System.Windows.Forms.Button();
			this.pnBuy.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblLicenseStatus
			// 
			this.lblLicenseStatus.AutoSize = true;
			this.lblLicenseStatus.Location = new System.Drawing.Point(21, 36);
			this.lblLicenseStatus.Name = "lblLicenseStatus";
			this.lblLicenseStatus.Size = new System.Drawing.Size(25, 13);
			this.lblLicenseStatus.TabIndex = 0;
			this.lblLicenseStatus.Text = "acc";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 10);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(67, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Input license";
			// 
			// txtLicenseInp
			// 
			this.txtLicenseInp.Location = new System.Drawing.Point(79, 7);
			this.txtLicenseInp.Name = "txtLicenseInp";
			this.txtLicenseInp.Size = new System.Drawing.Size(291, 20);
			this.txtLicenseInp.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 66);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(73, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "or click to buy";
			// 
			// btnBuy
			// 
			this.btnBuy.Location = new System.Drawing.Point(94, 63);
			this.btnBuy.Name = "btnBuy";
			this.btnBuy.Size = new System.Drawing.Size(70, 24);
			this.btnBuy.TabIndex = 5;
			this.btnBuy.Text = "Buy";
			this.btnBuy.UseVisualStyleBackColor = true;
			this.btnBuy.Click += new System.EventHandler(this.btnBuy_Click);
			// 
			// drpLicenseType
			// 
			this.drpLicenseType.FormattingEnabled = true;
			this.drpLicenseType.Location = new System.Drawing.Point(195, 63);
			this.drpLicenseType.Name = "drpLicenseType";
			this.drpLicenseType.Size = new System.Drawing.Size(100, 21);
			this.drpLicenseType.TabIndex = 6;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(170, 69);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(19, 13);
			this.label3.TabIndex = 7;
			this.label3.Text = "for";
			// 
			// pnBuy
			// 
			this.pnBuy.Controls.Add(this.btnInputLicense);
			this.pnBuy.Controls.Add(this.drpLicenseType);
			this.pnBuy.Controls.Add(this.label3);
			this.pnBuy.Controls.Add(this.label2);
			this.pnBuy.Controls.Add(this.txtLicenseInp);
			this.pnBuy.Controls.Add(this.btnBuy);
			this.pnBuy.Controls.Add(this.label1);
			this.pnBuy.Location = new System.Drawing.Point(15, 68);
			this.pnBuy.Name = "pnBuy";
			this.pnBuy.Size = new System.Drawing.Size(483, 100);
			this.pnBuy.TabIndex = 8;
			// 
			// btnInputLicense
			// 
			this.btnInputLicense.Location = new System.Drawing.Point(393, 4);
			this.btnInputLicense.Name = "btnInputLicense";
			this.btnInputLicense.Size = new System.Drawing.Size(70, 24);
			this.btnInputLicense.TabIndex = 8;
			this.btnInputLicense.Text = "OK";
			this.btnInputLicense.UseVisualStyleBackColor = true;
			this.btnInputLicense.Click += new System.EventHandler(this.btnInputLicense_Click);
			// 
			// LicenseItem
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.pnBuy);
			this.Controls.Add(this.lblLicenseStatus);
			this.Name = "LicenseItem";
			this.Size = new System.Drawing.Size(621, 357);
			this.pnBuy.ResumeLayout(false);
			this.pnBuy.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public System.Windows.Forms.Label lblLicenseStatus;
		private System.Windows.Forms.Label label2;
		public System.Windows.Forms.TextBox txtLicenseInp;
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.Button btnBuy;
		public System.Windows.Forms.ComboBox drpLicenseType;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel pnBuy;
		private Session session;
		public System.Windows.Forms.Button btnInputLicense;
	}
}
