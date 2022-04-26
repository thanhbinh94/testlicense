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
				UserIp = other.UserIp;
				UserID = other.UserID;
				IsTrialMode = other.IsTrialMode;
				LicenseKey = other.LicenseKey;
				IsExpired = other.IsExpired;
				UsedEndDate = other.UsedEndDate;
				//Password = other.Password;
				ConfigObjectDAO = other.ConfigObjectDAO;
				IsLocked = other.IsLocked;
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
