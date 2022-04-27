using System;
using System.Collections.Generic;
using System.Windows.Forms;
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

        #region Private method
        private void SetDataForLicenseTypeDrop()
        {
            listItemForDrop = QueryData.GetLicenseTypeForDropdown();
            drpLicenseType.DisplayMember = "Description";
            drpLicenseType.ValueMember = "LicenseType";
            drpLicenseType.DataSource = listItemForDrop;
            drpLicenseType.SelectedItem = 0;
        }
        #endregion
    }
}
