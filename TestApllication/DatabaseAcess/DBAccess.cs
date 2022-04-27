using TestApllication.Properties;

namespace TestApllication.DatabaseAcess
{
    internal class DBAccess
    {
        public static string ConnectionString
        {
            get
            {
                return Resources.ConnectionString ?? @"Data Source=BINHLT\TEST_DB;Initial Catalog=COMMON;User ID=sa;Password=Administrator@;";
            }
        }

        public static string DBCommonSchema
        {
            get
            {
                if (string.IsNullOrEmpty(Resources.DataLogCommon) || string.IsNullOrEmpty(Resources.Schema))
                {
                    return "COMMON.dbo";
                }
                string[] dbCommonInfo = new string[] { Resources.DataLogCommon, Resources.Schema };
                return string.Join(".", dbCommonInfo);
            }
        }
    }
}
