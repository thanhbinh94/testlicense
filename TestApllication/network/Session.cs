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

		public string UserUrl { get; set; }
		public string UserID { get; set; }
		public bool IsTrialMode { get; set; }
		public string LicenseKey { get; set; }
		public bool IsExpired { get; set; }
		public DateTime UsedEndDate { get; set; }
		//public string Password { get; set; }

		public ConfigObjectDAO ConfigObjectDAO { get; set; }

		public Session() { }

		public Session(Session other)
		{
			if (other != null)
			{
				instance.UserUrl = other.UserUrl;
				instance.UserID = other.UserID;
				instance.IsTrialMode = other.IsTrialMode;
				instance.LicenseKey = other.LicenseKey;
				instance.IsExpired = other.IsExpired;
				instance.UsedEndDate = other.UsedEndDate;
				//instance.Password = other.Password;
				instance.ConfigObjectDAO = other.ConfigObjectDAO;
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
