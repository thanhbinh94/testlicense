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
            bool loginCheck = QueryData.LoginCheck(loginItem1.txtInpUserId.Text, loginItem1.txtInpPass.Text, "127.0.0.1", out dt, out msgErr);
            if (!loginCheck)
            {
                MessageBox.Show(msgErr);
                return;
            }
            SetSessionInfo(dt);
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

            bool createAcc = QueryData.CreateAccount(loginItem1.txtInpUserId.Text, loginItem1.txtInpPass.Text, "127.0.0.1", configObjectDAO, out msgErr);
            if (!createAcc)
            {
                MessageBox.Show(msgErr);
                return;
            }

            DataTable dt = new DataTable();
            bool loginCheck = QueryData.LoginCheck(loginItem1.txtInpUserId.Text, loginItem1.txtInpPass.Text, "127.0.0.1", out dt, out msgErr);
            if (!loginCheck)
            {
                MessageBox.Show(msgErr);
                return;
            }
            SetSessionInfo(dt);
            SwitchToMain(session);
        }

        private void btnInputLicense_Click(object sender, EventArgs e)
        {
            bool validate = ValidateDataLicense(out string msgErr);
            if (!validate)
            {
                MessageBox.Show(msgErr);
                return;
            }

            bool inputLicense = QueryData.CreateInputLicense(session.UserID, licenseItem1.LicenseKeyInput, licenseItem1.LicenseKeyInput, session.UserIp, out msgErr);
            if (!inputLicense)
            {
                MessageBox.Show(msgErr);
                _ = QueryData.CheckInputTimesAndLock(session.UserID, session.UserIp, configObjectDAO.ConfigMaxInputTimes, ActionTypeEnum.ChangeInput, out bool isLocked, out _);
                session.IsLocked = isLocked;
                licenseItem1.Session = session;
                return;
            }
        }

        private void btnBuy_Click(object sender, EventArgs e)
        {
            string msgErr = string.Empty;
            bool buyLicense = QueryData.CreateBuyLicense(session.UserID, licenseItem1.LicenseTypeChoose, configObjectDAO, session.UserIp, out int dueDays, out string licenseKeyGen, out msgErr);
            if (!buyLicense)
            {
                MessageBox.Show(msgErr);
                return;
            }
            session.IsExpired = false;
            session.UsedEndDate = DateTimeUtil.AddDateTime(dueDays);
            session.LicenseKey = licenseKeyGen;
            licenseItem1.Session = session;
        }
        #endregion

        #region private method
        private bool ValidateDataLogin(ActionTypeEnum actionType, out string msgErr)
        {
            msgErr = String.Empty;
            if (string.IsNullOrEmpty(loginItem1.txtInpUserId.Text) || string.IsNullOrEmpty(loginItem1.txtInpPass.Text))
            {
                msgErr = (actionType == ActionTypeEnum.Login) ? Resources.MSG_ERR_005 : Resources.MSG_ERR_007;
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
            licenseItem1.OnClickBuy += btnBuy_Click;
            licenseItem1.Session = session;
        }

        private void SetSessionInfo(DataTable dtLogin)
        {
            session = new Session();
            session.UserID = dtLogin.Rows[0]["USER_ID"].ToString();
            // Hardcode
            session.UserIp = "127.0.0.1";
            if (dtLogin.Rows[0]["IS_LOCKED_IP"] != DBNull.Value)
            {
                session.IsLockedIp = (bool)dtLogin.Rows[0]["IS_LOCKED_IP"];
            }
            if (dtLogin.Rows[0]["IS_TRIAL_MODE"] != DBNull.Value)
            {
                session.IsTrialMode = (bool)dtLogin.Rows[0]["IS_TRIAL_MODE"];
            }
            if (dtLogin.Rows[0]["LICENSE_KEY"] != DBNull.Value)
            {
                session.LicenseKey = dtLogin.Rows[0]["LICENSE_KEY"].ToString();
            }
            if (dtLogin.Rows[0]["USED_END_DATE"] != DBNull.Value)
            {
                session.UsedEndDate = (DateTime)dtLogin.Rows[0]["USED_END_DATE"];
            }
            if (dtLogin.Rows[0]["IS_EXPIRED"] != DBNull.Value)
            {
                session.IsExpired = (bool)dtLogin.Rows[0]["IS_EXPIRED"];
            }
            session.ConfigObjectDAO = configObjectDAO;

            _ = QueryData.CheckInputTimesAndLock(session.UserID, session.UserIp, configObjectDAO.ConfigMaxInputTimes, null, out bool isLocked, out _);
            session.IsLocked = isLocked;
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
