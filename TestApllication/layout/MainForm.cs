using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;
using TestApllication.Properties;
using TestApllication.network;
using TestApllication.DatabaseAcess;
using TestApllication.DAO;

namespace TestApllication
{
	public partial class MainForm : Form
	{
		public ConfigObjectDAO configObjectDAO;
		public enum ActionType
        {
			Login = 0,
			CreateAccount = 1,
        }
		public MainForm()
		{
			InitializeComponent();
			licenseItem1.Visible = false;
			configObjectDAO = QueryData.GetConfigObject();
		}

        #region Event
        private void btnLogin_Click(object sender, EventArgs e)
		{
			string msgErr = string.Empty;
			bool validate = ValidateDataLogin(ActionType.Login, out msgErr);
			if (!validate)
			{
				MessageBox.Show(msgErr);
				return;
			}
			DataTable dt = new DataTable();
			bool loginCheck = QueryData.LoginCheck(loginItem1.txtInpUserId.Text, loginItem1.txtInpPass.Text, out dt, out msgErr);
			if (!loginCheck)
			{
				MessageBox.Show(msgErr);
				return;
			}
			session = new Session();
			session.UserID = dt.Rows[0]["USER_ID"].ToString();
			// Hardcode
			session.UserUrl = "127.0.0.1";
			session.ConfigObjectDAO = configObjectDAO;
		}

		private void btnCreateAcc_Click(object sender, EventArgs e)
		{
			string msgErr = string.Empty;
			bool validate = ValidateDataLogin(ActionType.CreateAccount, out msgErr);
			if (!validate)
			{
				MessageBox.Show(msgErr);
				return;
			}

			bool createAcc = QueryData.CreateAccount(loginItem1.txtInpUserId.Text, loginItem1.txtInpPass.Text, configObjectDAO, out msgErr);
			if (!createAcc)
			{
				MessageBox.Show(msgErr);
				return;
			}

			DataTable dt = new DataTable();
			bool loginCheck = QueryData.LoginCheck(loginItem1.txtInpUserId.Text, loginItem1.txtInpPass.Text, out dt, out msgErr);
			if (!loginCheck)
			{
				MessageBox.Show(msgErr);
				return;
			}
			session = new Session();
			session.UserID = dt.Rows[0]["USER_ID"].ToString();
		}
		#endregion

		#region private method
		private bool ValidateDataLogin(ActionType actionType, out string msgErr)
		{
			msgErr = String.Empty;
			if (string.IsNullOrEmpty(loginItem1.txtInpUserId.Text) || string.IsNullOrEmpty(loginItem1.txtInpPass.Text))
			{
				msgErr = (actionType == ActionType.Login)? Resources.MSG_ERR_005 : Resources.MSG_ERR_007;
				return false;
			}
			return true;
		}

		private void SwitchToMain()
        {
			loginItem1.
        }

		//private void GetIpAddress(out string userip)
		//{
		//	userip = Request.UserHostAddress;
		//	if (Request.UserHostAddress != null)
		//	{
		//		Int64 macinfo = new Int64();
		//		string macSrc = macinfo.ToString("X");
		//		if (macSrc == "0")
		//		{
		//			if (userip == "127.0.0.1")
		//			{
		//				Response.Write("visited Localhost!");
		//			}
		//			else
		//			{
		//				lblIPAdd.Text = userip;
		//			}
		//		}
		//	}
		//}
		#endregion
	}
}
