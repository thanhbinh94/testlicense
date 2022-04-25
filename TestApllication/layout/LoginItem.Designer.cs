namespace TestApllication.layout
{
	partial class LoginItem
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.btnCreateAcc = new System.Windows.Forms.Button();
            this.btnLogin = new System.Windows.Forms.Button();
            this.txtInpPass = new System.Windows.Forms.TextBox();
            this.txtInpUserId = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.btnCreateAcc);
            this.panel1.Controls.Add(this.btnLogin);
            this.panel1.Controls.Add(this.txtInpPass);
            this.panel1.Controls.Add(this.txtInpUserId);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(621, 357);
            this.panel1.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(294, 233);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "or";
            // 
            // button1
            // 
            this.btnCreateAcc.Location = new System.Drawing.Point(327, 228);
            this.btnCreateAcc.Name = "button1";
            this.btnCreateAcc.Size = new System.Drawing.Size(91, 23);
            this.btnCreateAcc.TabIndex = 5;
            this.btnCreateAcc.Text = "Create account";
            this.btnCreateAcc.UseVisualStyleBackColor = true;
            // 
            // txtLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(187, 228);
            this.btnLogin.Name = "txtLogin";
            this.btnLogin.Size = new System.Drawing.Size(91, 23);
            this.btnLogin.TabIndex = 4;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            // 
            // txtInpPass
            // 
            this.txtInpPass.Location = new System.Drawing.Point(165, 164);
            this.txtInpPass.Name = "txtInpPass";
            this.txtInpPass.Size = new System.Drawing.Size(293, 20);
            this.txtInpPass.TabIndex = 3;
            // 
            // txtInpName
            // 
            this.txtInpUserId.Location = new System.Drawing.Point(165, 118);
            this.txtInpUserId.Name = "txtInpName";
            this.txtInpUserId.Size = new System.Drawing.Size(293, 20);
            this.txtInpUserId.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(76, 167);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Password";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(76, 121);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "UserName";
            // 
            // LoginItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "LoginItem";
            this.Size = new System.Drawing.Size(621, 357);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		public System.Windows.Forms.Button btnLogin;
        public System.Windows.Forms.TextBox txtInpPass;
        public System.Windows.Forms.TextBox txtInpUserId;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Button btnCreateAcc;
		private System.Windows.Forms.Label label3;
	}
}
