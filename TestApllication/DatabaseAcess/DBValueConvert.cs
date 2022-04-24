using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestApllication.DatabaseAcess
{
	sealed class DBValueConvert
	{
		public enum DateTimeType
		{
			DateTime,
			Date,
			Month
		}

		public static object ToNvarchar(string value)
		{
			if (string.IsNullOrEmpty(value)) {
				return DBNull.Value;
			}
			return value;
		}

		public static object ToDateTime(string value)
		{
			return ToDateTime(value, DateTimeType.Date);
		}

		public static object ToDateTime(string value, DateTimeType type)
		{
			if (value == null || value.Length == 0)
			{
				return DBNull.Value;
			}
			else
			{
				switch (type)
				{
					case DateTimeType.DateTime:
					case DateTimeType.Date:
						return DateTime.Parse(value);
					case DateTimeType.Month:
						return DateTime.Parse(value + "/01");
					default:
						return DBNull.Value;
				}
			}
		}

		public static object ToInt(string value)
		{
			int result;
			bool canParse = int.TryParse(value, out result);
			if (canParse)
			{
				return result;
			}
			return DBNull.Value;
		}
	}
}
