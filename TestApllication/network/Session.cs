using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestApllication.DAO;

namespace TestApllication.network
{
	public class Session
	{
		private static Session instance;

		public string UserIp { get; set; }
		public string UserID { get; set; }
		public bool IsTrialMode { get; set; }
		public string LicenseKey { get; set; }
		public bool IsExpired { get; set; }
		public DateTime UsedEndDate { get; set; }
		public bool IsLocked { get; set; }
		//public string Password { get; set; }

		public ConfigObjectDAO ConfigObjectDAO { get; set; }

		public Session() { }

		public Session(Session other)
		{
			if (other != null)
			{
				instance.UserIp = other.UserIp;
				instance.UserID = other.UserID;
				instance.IsTrialMode = other.IsTrialMode;
				instance.LicenseKey = other.LicenseKey;
				instance.IsExpired = other.IsExpired;
				instance.UsedEndDate = other.UsedEndDate;
				//instance.Password = other.Password;
				instance.ConfigObjectDAO = other.ConfigObjectDAO;
				instance.IsLocked = other.IsLocked;
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
