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
		//public string Password { get; set; }

		public ConfigObjectDAO ConfigObjectDAO { get; set; }

		public Session() { }

		public Session(Session other)
		{
			if (other != null)
			{
				UserUrl = other.UserUrl;
				UserID = other.UserID;
				//Password = other.Password;
				ConfigObjectDAO = other.ConfigObjectDAO;
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
