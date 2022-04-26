using System;
using System.Text;

namespace TestApllication.util
{
	internal class StringUtil
	{
		public static string CreateRandomString(int length)
		{
			if (length == 0) return string.Empty;
			StringBuilder sb = new StringBuilder();
			var rand = new Random();

			for (int i = 0; i < length; i++)
			{
				int randChar = rand.Next(0, 255);
				while (randChar < 48 || (randChar > 57 && randChar < 65) || (randChar > 90 && randChar < 97) || randChar > 122)
				{
					randChar = rand.Next(0, 255);
				}
				sb.Append((char)randChar);
			}
			return sb.ToString();
		}

		public static bool ToBoolValue(string value)
		{
			if (value != "0" && value != "1")
			{
				return false;
			}
			return value == "1";
		}
	}
}
