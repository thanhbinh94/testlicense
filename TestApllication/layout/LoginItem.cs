using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TestApllication.Properties;
using TestApllication.network;
using TestApllication.DatabaseAcess;

namespace TestApllication.layout
{
	public partial class LoginItem : UserControl
	{
		public Session session;
		public LoginItem(Session session)
		{
			InitializeComponent();
			this.session = session;
		}

        private void txtLogin_Click(object sender, EventArgs e)
        {
			string msgErr = string.Empty;
			bool validate = ValidateLogin(out msgErr);
			if (!validate)
            {
				MessageBox.Show(msgErr);
				return;
            }
			DataTable dt = new DataTable();
			bool loginCheck = QueryData.LoginCheck(txtInpUserId.Text, txtInpPass.Text, out dt, out msgErr);
			if (!loginCheck)
            {
				MessageBox.Show(msgErr);
				return;
			}
			session = new Session();
			session.UserID = dt.Rows[0]["USER_ID"].ToString();
		}


        #region private method
		private bool ValidateLogin(out string msgErr)
        {
			msgErr = String.Empty;
			if (string.IsNullOrEmpty(txtInpUserId.Text) || string.IsNullOrEmpty(txtInpPass.Text)) 
			{
                msgErr = Resources.MSG_ERR_005;
                return false;
			}
			return true;
        }
        #endregion
    }
}
