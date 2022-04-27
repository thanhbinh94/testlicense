using System;
using TestApllication.DAO;

namespace TestApllication.network
{
    public class Session
    {
        private static Session instance;

        public string UserIp { get; set; }
        public string UserID { get; set; }
        public object IsTrialMode { get; set; }
        public object LicenseKey { get; set; }
        public object IsExpired { get; set; }
        public object UsedEndDate { get; set; }
        public object IsLocked { get; set; }
        public object IsLockedIp { get; set; }
        //public string Password { get; set; }

        public ConfigObjectDAO ConfigObjectDAO { get; set; }

        public Session() { }

        public Session(Session other)
        {
            if (other != null)
            {
                UserIp = other.UserIp;
                UserID = other.UserID;
                IsTrialMode = other.IsTrialMode;
                LicenseKey = other.LicenseKey;
                IsExpired = other.IsExpired;
                UsedEndDate = other.UsedEndDate;
                //Password = other.Password;
                ConfigObjectDAO = other.ConfigObjectDAO;
                IsLocked = other.IsLocked;
                IsLockedIp = other.IsLockedIp;
                instance = other;
            }
        }

        public static Session getSession()
        {
            if (instance == null)
            {
                instance = new Session();
            }
            return instance;
        }
    }
}
