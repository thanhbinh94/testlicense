using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TestApllication.layout
{
    public partial class LoginItem : UserControl
    {
		#region Properties
		public EventHandler OnLogin;
		public EventHandler OnCreateAccount;
		#endregion

		public LoginItem()
        {
            InitializeComponent();
        }

		#region Event
		private void btnLogin_Click(object sender, EventArgs e)
		{
			OnLogin?.Invoke(this, e);
		}

		private void btnCreateAcc_Click(object sender, EventArgs e)
		{
			OnCreateAccount?.Invoke(this, e);
		}
		#endregion
	}
}
