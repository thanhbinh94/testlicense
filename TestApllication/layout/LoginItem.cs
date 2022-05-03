using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TestApllication.layout
{
    public partial class LoginItem : UserControl
    {
		#region Properties
		public EventHandler Login;
		public EventHandler CreateAccount;
		#endregion

		public LoginItem()
        {
            InitializeComponent();
        }

		#region Event
		protected virtual void OnLogin(object sender, EventArgs e)
		{
			Login?.Invoke(this, e);
		}

		protected virtual void OnCreateAccount(object sender, EventArgs e)
		{
			CreateAccount?.Invoke(this, e);
		}
		#endregion
	}
}
