using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApllication.DAO
{
	internal class LicenseTypeItemDAO
	{
		private string licenseType;
		private string description;

		public string LicenseTypeId
		{
			get
			{
				return licenseType ?? string.Empty;
			}
			set
			{
				licenseType = value;
			}
		}

		public string Description
		{
			get
			{
				return description ?? string.Empty;
			}
			set
			{
				description = value;
			}
		}
	}
}
