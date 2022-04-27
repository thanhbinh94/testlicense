using System;
using System.Data;
using System.Windows.Forms;
using TestApllication.DAO;
using TestApllication.DatabaseAcess;
using TestApllication.network;
using TestApllication.Properties;
using TestApllication.util;

namespace TestApllication
{
    public partial class MainForm : Form
    {
        public ConfigObjectDAO configObjectDAO;

        public MainForm()
        {
            InitializeComponent();
			loginItem1.Visible = true;
			loginItem1.OnLogin += btnLogin_Click;
			loginItem1.OnCreateAccount += btnCreateAcc_Click;
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
            bool loginCheck = QueryData.LoginCheck(loginItem1.UserIdInp, loginItem1.PasswordInp, "127.0.0.1", out dt, out msgErr);
            if (!loginCheck)
            {
                MessageBox.Show(msgErr);
                return;
            }
            SetSessionInfo(dt, true);
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

            bool createAcc = QueryData.CreateAccount(loginItem1.UserIdInp, loginItem1.PasswordInp, "127.0.0.1", configObjectDAO, out msgErr);
            if (!createAcc)
            {
                MessageBox.Show(msgErr);
                return;
            }

            DataTable dt = new DataTable();
            bool loginCheck = QueryData.LoginCheck(loginItem1.UserIdInp, loginItem1.PasswordInp, "127.0.0.1", out dt, out msgErr);
            if (!loginCheck)
            {
                MessageBox.Show(msgErr);
                return;
            }
            SetSessionInfo(dt, true);
            SwitchToMain(session);
        }

        private void btnInputLicense_Click(object sender, EventArgs e)
        {
			string msgErr;
			bool validate = ValidateDataLicense(out msgErr);
            if (!validate)
            {
                MessageBox.Show(msgErr);
                return;
            }

			DataTable dtLicInfo;
            bool inputLicense = QueryData.CreateInputLicense(session.UserID, session.UserIp, licenseItem1.LicenseKeyInput, out dtLicInfo, out msgErr);
            if (!inputLicense)
            {
                MessageBox.Show(msgErr);
                bool isLocked = QueryData.CheckInputTimesAndLock(session.UserID, session.UserIp, configObjectDAO.ConfigMaxInputTimes, ActionTypeEnum.ChangeInput, out msgErr);
				if (!string.IsNullOrEmpty(msgErr))
				{
					MessageBox.Show(msgErr);
					return;
				}
                session.IsLocked = isLocked;
            }
			else
			{
				SetSessionInfo(dtLicInfo, false);
			}

			licenseItem1.Session = session;
		}

        private void btnBuy_Click(object sender, EventArgs e)
        {
            string msgErr = string.Empty;
			int dueDays;
			string licenseKeyGen;
			bool buyLicense = QueryData.CreateBuyLicense(session.UserID, licenseItem1.LicenseTypeChoose, configObjectDAO, session.UserIp, out dueDays, out licenseKeyGen, out msgErr);
            if (!buyLicense)
            {
                MessageBox.Show(msgErr);
                return;
            }
			session.IsTrialMode = false;
            session.IsExpired = false;
            session.UsedEndDate = DateTimeUtil.AddDateTime(dueDays);
            session.LicenseKey = licenseKeyGen;
            licenseItem1.Session = session;
        }
        #endregion

        #region private method
        private bool ValidateDataLogin(ActionTypeEnum actionType, out string msgErr)
        {
            msgErr = string.Empty;
            if (string.IsNullOrEmpty(loginItem1.UserIdInp) || string.IsNullOrEmpty(loginItem1.PasswordInp))
            {
                msgErr = (actionType == ActionTypeEnum.Login) ? Resources.MSG_ERR_005 : Resources.MSG_ERR_007;
                return false;
            }
            return true;
        }

        private bool ValidateDataLicense(out string msgErr)
        {
            msgErr = string.Empty;
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
            licenseItem1.OnClickBuy += btnBuy_Click;
            licenseItem1.Session = session;
        }

        private void SetSessionInfo(DataTable dtInfo, bool needCheckLock)
        {
            session = new Session();
			if (dtInfo.Rows[0]["USER_ID"] != DBNull.Value)
			{
				session.UserID = dtInfo.Rows[0]["USER_ID"].ToString();
			}
            // Hardcode
            session.UserIp = "127.0.0.1";
            if (dtInfo.Rows[0]["IS_LOCKED_IP"] != DBNull.Value)
            {
                session.IsLockedIp = (bool)dtInfo.Rows[0]["IS_LOCKED_IP"];
            }
            if (dtInfo.Rows[0]["IS_TRIAL_MODE"] != DBNull.Value)
            {
                session.IsTrialMode = (bool)dtInfo.Rows[0]["IS_TRIAL_MODE"];
            }
            if (dtInfo.Rows[0]["LICENSE_KEY"] != DBNull.Value)
            {
                session.LicenseKey = dtInfo.Rows[0]["LICENSE_KEY"].ToString();
            }
            if (dtInfo.Rows[0]["USED_END_DATE"] != DBNull.Value)
            {
                session.UsedEndDate = (DateTime)dtInfo.Rows[0]["USED_END_DATE"];
            }
            if (dtInfo.Rows[0]["IS_EXPIRED"] != DBNull.Value)
            {
                session.IsExpired = (bool)dtInfo.Rows[0]["IS_EXPIRED"];
            }
            session.ConfigObjectDAO = configObjectDAO;

			if (needCheckLock)
			{
				string msgErr;
				bool isLocked = QueryData.CheckInputTimesAndLock(session.UserID, session.UserIp, configObjectDAO.ConfigMaxInputTimes, null, out msgErr);
				if (!string.IsNullOrEmpty(msgErr))
				{
					MessageBox.Show(msgErr);
					return;
				}
				session.IsLocked = isLocked;
			}
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
