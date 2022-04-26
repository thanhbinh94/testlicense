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
using TestApllication.util;

namespace TestApllication
{
	public partial class MainForm : Form
	{
		public ConfigObjectDAO configObjectDAO;

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
			bool validate = ValidateDataLogin(ActionTypeEnum.Login, out msgErr);
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
			session.UserIp = "127.0.0.1";
			session.ConfigObjectDAO = configObjectDAO;

			bool isLocked = false;
			int inputTime = QueryData.CheckInputTimesAndLock(session.UserID, session.UserIp, configObjectDAO.ConfigMaxInputTimes, null, out isLocked, out msgErr);
			session.IsLocked = isLocked;
			SwitchToMain(session);
		}

		private void btnCreateAcc_Click(object sender, EventArgs e)
		{
			string msgErr = string.Empty;
			bool validate = ValidateDataLogin(ActionTypeEnum.CreateAccount, out msgErr);
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
			session.IsTrialMode = StringUtil.ToBoolValue(dt.Rows[0]["IS_TRIAL_MODE"].ToString());
			session.LicenseKey = dt.Rows[0]["LICENSE_KEY"].ToString();
			session.UsedEndDate = (DateTime) dt.Rows[0]["USED_END_DATE"];
			session.IsExpired = StringUtil.ToBoolValue(dt.Rows[0]["IS_EXPIRED"].ToString());
			bool isLocked = false;
			int inputTime = QueryData.CheckInputTimesAndLock(session.UserID, session.UserIp, configObjectDAO.ConfigMaxInputTimes, null, out isLocked, out msgErr);
			session.IsLocked = isLocked;
			SwitchToMain(session);
		}

		private void btnInputLicense_Click(object sender, EventArgs e)
		{
			string msgErr = string.Empty;
			bool validate = ValidateDataLicense(out msgErr);
			if (!validate)
			{
				MessageBox.Show(msgErr);
				return;
			}

			bool inputLicense = QueryData.CreateInputLicense(session.UserID, licenseItem1.LicenseKeyInput, licenseItem1.LicenseKeyInput, session.UserIp, out msgErr);
			if (!inputLicense)
			{
				MessageBox.Show(msgErr);
				bool isLocked = false;
				int countInputTime = QueryData.CheckInputTimesAndLock(session.UserID, session.UserIp, configObjectDAO.ConfigMaxInputTimes, ActionTypeEnum.ChangeInput, out isLocked, out msgErr);
				session.IsLocked = isLocked;
				licenseItem1.Session = session;
				return;
			}
		}

		private void btnBuy_Click(object sender, EventArgs e)
		{
			string msgErr = string.Empty;

			bool buyLicense = QueryData.CreateBuyLicense(session.UserID, licenseItem1.LicenseTypeChoose, configObjectDAO, session.UserIp, out msgErr);
			if (!buyLicense)
			{
				MessageBox.Show(msgErr);
				return;
			}
		}
		#endregion

		#region private method
		private bool ValidateDataLogin(ActionTypeEnum actionType, out string msgErr)
		{
			msgErr = String.Empty;
			if (string.IsNullOrEmpty(loginItem1.txtInpUserId.Text) || string.IsNullOrEmpty(loginItem1.txtInpPass.Text))
			{
				msgErr = (actionType == ActionTypeEnum.Login)? Resources.MSG_ERR_005 : Resources.MSG_ERR_007;
				return false;
			}
			return true;
		}

		private bool ValidateDataLicense(out string msgErr)
		{
			msgErr = String.Empty;
			if (string.IsNullOrEmpty(licenseItem1.LicenseKeyInput))
			{
				msgErr = Resources.MSG_ERR_008;
				return false;
			}
			return true;
		}

		private void SwitchToMain(Session session)
        {
			loginItem1.Visible = false;
			licenseItem1.Visible = true;
			licenseItem1.OnClickInput += btnInputLicense_Click;
			licenseItem1.OnClickBuy += btnInputLicense_Click;
			licenseItem1.Session = session;
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
