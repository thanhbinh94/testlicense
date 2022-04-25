using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestApllication.util
{
    internal class DateTimeUtil
    {
        public static DateTime AddDateTime(int days)
        {
            return AddDateTime(DateTime.Now, days);
        }

        public static DateTime AddDateTime(DateTime date, int days)
        {
            return date.AddDays(days);
        }

		public static int GetDaysRemain(DateTime fromDate, DateTime toDate)
		{
			return (toDate - fromDate).Days;
		}
    }
}
