using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TestApllication.network;
using TestApllication.DAO;
using TestApllication.DatabaseAcess;

namespace TestApllication.layout
{
	public partial class LicenseItem : UserControl
	{
		#region Properties
		private List<LicenseTypeItemDAO> listItemForDrop = new List<LicenseTypeItemDAO>();
		public EventHandler OnClickInput;
		public EventHandler OnClickBuy;
		#endregion

		public LicenseItem()
		{
			InitializeComponent();
			//drpLicenseType.DataSource
			listItemForDrop = QueryData.GetLicenseTypeForDropdown();
			listItemForDrop.Sort();
			drpLicenseType.DataSource = listItemForDrop;
			drpLicenseType.DisplayMember = "Description";
			drpLicenseType.ValueMember = "LicenseType";
			drpLicenseType.SelectedItem = 0;
		}

		#region Event
		private void btnInputLicense_Click(object sender, EventArgs e)
		{
			OnClickInput?.Invoke(this, e);
		}

		private void btnBuy_Click(object sender, EventArgs e)
		{
			OnClickBuy?.Invoke(this, e);
		}
		#endregion
	}
}
