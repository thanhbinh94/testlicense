using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TestApllication
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());
            MainForm mainForm = new MainForm();
            for (DialogResult dlg = mainForm.ShowDialog(); dlg == DialogResult.Yes; dlg = mainForm.ShowDialog())
            {
                if (mainForm.session != null)
                {
					mainForm.loginItem1.Visible = false;
					mainForm.licenseItem1.Visible = true;
				}
            }
        }
	}
}
