using System;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using TestApllication.Properties;
using TestApllication.util;
using TestApllication.DAO;

namespace TestApllication.DatabaseAcess
{
	internal class QueryData
	{
		#region Public method
		public static bool LoginCheck(string useID, string password, out DataTable dt, out string msgErr)
		{
			using (SqlConnection cn = new SqlConnection(DBAccess.ConnectionString))
			{
				msgErr = string.Empty;
				dt = new DataTable();
				string loginQuery = LoginQuery();

				cn.Open();
				SqlCommand cm = new SqlCommand(loginQuery, cn);
				cm.CommandTimeout = 120;
				cm.Parameters.Add("@userid", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(useID);
				cm.Parameters.Add("@password", SqlDbType.NVarChar, 50).Value = DBValueConvert.ToNvarchar(password);

				try
				{
					SqlDataReader dr = cm.ExecuteReader();
					if (dr.Read())
					{
						dt.Columns.Add("USER_ID");
						dt.Columns.Add("LICENSE_KEY");
						dt.Columns.Add("USED_END_DATE");
						dt.Columns.Add("IS_EXPIRED");
						dt.BeginInit();
						DataRow dRow = dt.NewRow();
						dRow["USER_ID"] = dr["USER_ID"].ToString();
						dt.Rows.Add(dRow);
						dt.EndInit();
						dt.AcceptChanges();
						dr.Close();

						string getLicenseHasQuery = GetLicenseHashQuery();
						cm = new SqlCommand(getLicenseHasQuery, cn);
						cm.CommandTimeout = 120;
						cm.Parameters.Add("@useridHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptStringToBase64(useID));
						SqlDataReader dr2 = cm.ExecuteReader();
						if (dr2.Read())
						{
							string licenseHash = dr2["LICENSE_KEY_HASH"].ToString();
							dr2.Close();

							string getLicenseInfoQuery = GetLicenseInfoQuery();
							cm = new SqlCommand(getLicenseInfoQuery, cn);
							cm.CommandTimeout = 120;
							cm.Parameters.Add("@licenseKey", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(EncryptUtil.DecryptStringFromBase64(licenseHash));

							SqlDataReader dr3 = cm.ExecuteReader();
							if (dr3.Read())
							{
								dt.BeginLoadData();
								dt.Rows[0]["LICENSE_KEY"] = dr3["LICENSE_KEY"].ToString();
								dt.Rows[0]["IS_EXPIRED"] = dr3["IS_EXPIRED"].ToString();
								dt.Rows[0]["USED_END_DATE"] = dr3["USED_END_DATE"].ToString();
								dt.EndLoadData();
								dt.AcceptChanges();
							}
							dr3.Close();
						}
						return true;
					}
					msgErr = Resources.MSG_ERR_001;
					return false;
				}
				catch (SqlException)
				{
					msgErr = Resources.MSG_ERR_002;
					return false;
				}
				finally
				{
					cn.Close();
				}
			}
		}

		public static bool CreateAccount(string useID, string password, ConfigObjectDAO configObjectDAO, out string msgErr)
		{
			using (SqlConnection cn = new SqlConnection(DBAccess.ConnectionString))
			{
				msgErr = string.Empty;
				string loginQuery = CheckUserExistsQuery();

				cn.Open();
				SqlCommand cm = new SqlCommand(loginQuery, cn);
				cm.CommandTimeout = 120;
				cm.Parameters.Add("@userid", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(useID);

				try
				{
                    #region Create user
                    SqlDataReader dr = cm.ExecuteReader();
					if (dr.Read())
					{
						msgErr = Resources.MSG_ERR_003;
						return false;
					}

					string createUserQuery = CreateUserQuery();
					cm = new SqlCommand(createUserQuery, cn);
					cm.CommandTimeout = 120;
					cm.Parameters.Add("@userid", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(useID);
					cm.Parameters.Add("@password", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(password);
					cm.Parameters.Add("@passwordhash", SqlDbType.NVarChar, 500).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptStringToBase64(password));

					cm.ExecuteNonQuery();
                    #endregion

                    #region Create license
                    string licenseKeyGen = GenerateLicenseKey(configObjectDAO);
					string licenseTypeId = GetLicenseTypeId(cn);

					string createLicenseQuery = CreateLicenseQuery();
					cm = new SqlCommand(createLicenseQuery, cn);
					cm.CommandTimeout = 120;
					cm.Parameters.Add("@licenseKey", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(licenseKeyGen);
					cm.Parameters.Add("@licenseType", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(licenseTypeId);
					cm.Parameters.Add("@isUsed", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(true);
					cm.Parameters.Add("@usedStartDate", SqlDbType.NVarChar, 30).Value = DateTime.Now;
					cm.Parameters.Add("@usedEndDate", SqlDbType.NVarChar, 30).Value = DateTimeUtil.AddDateTime(configObjectDAO.ConfigTrialDueDays);
					cm.Parameters.Add("@isExpired", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(false);

					cm.ExecuteNonQuery();
					#endregion

					#region Create user-license mapping
					string createUserLicenseMapppingQuery = CreateUserLicenseMappingQuery();
					cm = new SqlCommand(createUserLicenseMapppingQuery, cn);
					cm.CommandTimeout = 120;
					cm.Parameters.Add("@userIdHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptStringToBase64(licenseKeyGen));
					cm.Parameters.Add("@licenseKeyHash", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptStringToBase64(licenseKeyGen));
					// Hardcode
					cm.Parameters.Add("@hashKey", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar("127.0.0.1");

					cm.ExecuteNonQuery();
					#endregion

					return true;
				}
				catch (SqlException)
				{
					msgErr = Resources.MSG_ERR_004;
					return false;
				}
				finally
				{
					cn.Close();
				}
			}
		}

		public static string GetLicenseTypeId(SqlConnection cn)
        {
			string GetLicenseTypeIDQuery = GetLicenseTypeByTrialQuery();
			SqlCommand cm = new SqlCommand(GetLicenseTypeIDQuery, cn);
			cm.CommandTimeout = 120;
			cm.Parameters.Add("@isTrial", SqlDbType.NVarChar, 1).Value = DBValueConvert.ToNvarchar(true);
			try
			{
				SqlDataReader dr = cm.ExecuteReader();
				if (dr.Read())
				{
					return dr["LICENSE_TYPE_ID"].ToString();
				}

				return string.Empty;
			}
			catch (SqlException)
			{
				return string.Empty;
			}
		}

		public static ConfigObjectDAO GetConfigObject()
		{
			using (SqlConnection cn = new SqlConnection(DBAccess.ConnectionString))
			{
				ConfigObjectDAO configObjectDAO = new ConfigObjectDAO();
				string getConfigObject = GetConfigQuery();

				cn.Open();
				SqlCommand cm = new SqlCommand(getConfigObject, cn);
				cm.CommandTimeout = 120;
				cm.Parameters.Add("@configID", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(Settings.Default.CONFIG_CONFIGID);

				try
				{
					SqlDataReader dr = cm.ExecuteReader();
					if (dr.Read())
					{
						configObjectDAO.ConfigTrialDueDays = int.Parse(dr["TRIAL_DUE_DAYS"].ToString());
						configObjectDAO.ConfigNumberDigitPerUnit = int.Parse(dr["CONFIG_NUM_DIGIT_PER_UNIT"].ToString());
						configObjectDAO.ConfigNumberOfUnit = int.Parse(dr["CONFIG_NUM_UNIT"].ToString());
						configObjectDAO.ConfigLicenseSuffix = dr["CONFIG_LICENSE_SUFFIX"].ToString();
					}

					return configObjectDAO;
				}
				catch (SqlException)
				{
					return new ConfigObjectDAO();
				}
				finally
				{
					cn.Close();
				}
			}
		}
		#endregion


		#region Private method
		private static string LoginQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT USER_ID FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[USER_INFORMATION] ");
			sb.Append("WHERE USER_ID = @userid AND PASSWORD = @password");
			return sb.ToString();
		}

		private static string GetLicenseHashQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT LICENSE_KEY_HASH FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[USER_LICENSE_MAPPING] ");
			sb.Append("WHERE USER_ID_HASH = @useridHash");
			return sb.ToString();
		}

		private static string GetLicenseInfoQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT LICENSE_KEY, IS_EXPIRED, USED_END_DATE FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[LICENSE] ");
			sb.Append("WHERE LICENSE_KEY = @licenseKey");
			return sb.ToString();
		}

		private static string GetLicenseTypeByTrialQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT LICENSE_TYPE_ID FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[LICENSE] ");
			sb.Append("WHERE IS_TRIAL = @isTrial");
			return sb.ToString();
		}

		private static string CheckUserExistsQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT USERNAME FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[USER_INFORMATION] ");
			sb.Append("WHERE USER_ID = @userid");
			return sb.ToString();
		}

		private static string GetConfigQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT TRIAL_DUE_DAYS, CONFIG_NUM_DIGIT_PER_UNIT, CONFIG_NUM_UNIT,  FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[CONFIG] ");
			sb.Append("WHERE CONFIG_ID = @configID");
			return sb.ToString();
		}

		private static string CreateUserQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("INSERT INTO ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[USER_INFORMATION] ");
			sb.Append("VALUES (@userid, @password, @passwordhash, '1', '0', GETDATE())");
			return sb.ToString();
		}

		private static string CreateLicenseQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("INSERT INTO ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[LICENSE] ");
			sb.Append("VALUES (@licenseKey, @licenseType, @isUsed, @usedStartDate, @usedEndDate, @isExpired, GETDATE())");
			return sb.ToString();
		}

		private static string CreateUserLicenseMappingQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("INSERT INTO ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[USER_LICENSE_MAPPING] ");
			sb.Append("VALUES (@userIdHash, @licenseKeyHash, @hashKey, GETDATE())");
			return sb.ToString();
		}

		private static string GenerateLicenseKey(ConfigObjectDAO configObjectDAO)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < configObjectDAO.ConfigNumberOfUnit; i++)
            {
				sb.Append(StringUtil.CreateRandomString(configObjectDAO.ConfigNumberDigitPerUnit)).Append(configObjectDAO.ConfigLicenseSuffix);
            }

            return sb.ToString();
		}
		#endregion
	}
}
