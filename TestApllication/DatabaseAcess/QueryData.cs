using System;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
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
						dt = MakeDataTableLogin();

						dt.BeginInit();
						DataRow dRow = dt.NewRow();
						dRow["USER_ID"] = dr["USER_ID"].ToString();
						dRow["LICENSE_KEY"] = string.Empty;
						dRow["IS_EXPIRED"] = "1";
						dRow["USED_END_DATE"] = DateTime.Today;
						dt.Rows.Add(dRow);
						dt.EndInit();
						dt.AcceptChanges();
						dr.Close();

						string getLicenseHasQuery = GetLicenseHashQuery();
						cm = new SqlCommand(getLicenseHasQuery, cn);
						cm.CommandTimeout = 120;
						cm.Parameters.Add("@useridHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(useID));
						SqlDataReader dr2 = cm.ExecuteReader();
						if (dr2.Read())
						{
							string licenseHash = dr2["LICENSE_KEY_HASH"].ToString();
							dr2.Close();

							string getLicenseInfoQuery = GetLicenseInfoQuery();
							cm = new SqlCommand(getLicenseInfoQuery, cn);
							cm.CommandTimeout = 120;
							cm.Parameters.Add("@licenseKey", SqlDbType.NVarChar, 50).Value = DBValueConvert.ToNvarchar(EncryptUtil.DecryptASCIIStringFromBase64(licenseHash));

							SqlDataReader dr3 = cm.ExecuteReader();
							if (dr3.Read())
							{
								dt.BeginLoadData();
								dt.Rows[0]["LICENSE_KEY"] = dr3["LICENSE_KEY"].ToString();
								dt.Rows[0]["IS_EXPIRED"] = dr3["IS_EXPIRED"].ToString();
								dt.Rows[0]["USED_END_DATE"] = (DateTime)dr3["USED_END_DATE"];
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

		public static string GetLicenseTypeId(SqlConnection cn, SqlTransaction str)
        {
			string GetLicenseTypeIDQuery = GetLicenseTypeByTrialQuery();
			SqlCommand cm = new SqlCommand(GetLicenseTypeIDQuery, cn, str);
			cm.CommandTimeout = 120;
			cm.Parameters.Add("@isTrial", SqlDbType.NVarChar, 1).Value = DBValueConvert.ToNvarchar(true);
			SqlDataReader dr = cm.ExecuteReader();
			try
			{
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
			finally
			{
				dr.Close();
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
						configObjectDAO.ConfigMaxInputTimes = int.Parse(dr["CONFIG_MAX_INPUT_TIMES"].ToString());
					}
					dr.Close();
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

		public static List<LicenseTypeItemDAO> GetLicenseTypeForDropdown() 
		{
			using (SqlConnection cn = new SqlConnection(DBAccess.ConnectionString))
			{
				List<LicenseTypeItemDAO> listLicenseTypeDrop = new List<LicenseTypeItemDAO>();
				string getLicenseTypeForDrop = GetLicenseTypeForDropdownQuery();

				cn.Open();
				SqlCommand cm = new SqlCommand(getLicenseTypeForDrop, cn);
				cm.CommandTimeout = 120;

				try
				{
					SqlDataReader dr = cm.ExecuteReader();
					while (dr.Read())
					{
						LicenseTypeItemDAO itemDAO = new LicenseTypeItemDAO();
						itemDAO.LicenseTypeId = dr["LICENSE_TYPE_ID"].ToString();
						itemDAO.Description = dr["DESCRIPTION"].ToString();
						listLicenseTypeDrop.Add(itemDAO);
					}
					dr.Close();
					return listLicenseTypeDrop;
				}
				catch (SqlException)
				{
					return new List<LicenseTypeItemDAO>();
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
				SqlTransaction str = cn.BeginTransaction();
				SqlCommand cm = new SqlCommand(loginQuery, cn, str);
				cm.CommandTimeout = 120;
				cm.Parameters.Add("@userid", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(useID);
				SqlDataReader dr = cm.ExecuteReader();

				try
				{
					#region Check exist
					if (dr.Read())
					{
						msgErr = Resources.MSG_ERR_003;
						return false;
					}
					dr.Close();
					#endregion

					#region Create user
					string createUserQuery = CreateUserQuery();
					cm = new SqlCommand(createUserQuery, cn, str);
					cm.CommandTimeout = 120;
					cm.Parameters.Add("@userid", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(useID);
					cm.Parameters.Add("@password", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(password);
					cm.Parameters.Add("@passwordhash", SqlDbType.NVarChar, 500).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(password));

					cm.ExecuteNonQuery();
					#endregion

					#region Create license
					string licenseKeyGen = GenerateLicenseKey(configObjectDAO);
					string licenseTypeId = GetLicenseTypeId(cn, str);

					string createLicenseQuery = CreateLicenseQuery();
					cm = new SqlCommand(createLicenseQuery, cn, str);
					cm.CommandTimeout = 120;
					cm.Parameters.Add("@licenseKey", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(licenseKeyGen);
					cm.Parameters.Add("@licenseType", SqlDbType.NVarChar, 10).Value = DBValueConvert.ToNvarchar(licenseTypeId);
					cm.Parameters.Add("@isUsed", SqlDbType.NVarChar, 1).Value = DBValueConvert.ToNvarchar(true);
					cm.Parameters.Add("@usedStartDate", SqlDbType.DateTime).Value = DateTime.Now;
					cm.Parameters.Add("@usedEndDate", SqlDbType.DateTime).Value = DateTimeUtil.AddDateTime(configObjectDAO.ConfigTrialDueDays);
					cm.Parameters.Add("@isExpired", SqlDbType.NVarChar, 1).Value = DBValueConvert.ToNvarchar(false);

					cm.ExecuteNonQuery();
					#endregion

					#region Create user-license mapping
					string createUserLicenseMapppingQuery = CreateUserLicenseMappingQuery();
					string[] infoArr = new string[] { useID, licenseKeyGen, "127.0.0.1" };
					cm = new SqlCommand(createUserLicenseMapppingQuery, cn, str);
					cm.CommandTimeout = 120;
					cm.Parameters.Add("@userIdHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(useID));
					cm.Parameters.Add("@licenseKeyHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(licenseKeyGen));
					// Hardcode
					cm.Parameters.Add("@hashKey", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(string.Join("", infoArr)));

					cm.ExecuteNonQuery();
					#endregion

					str.Commit();
					return true;
				}
				catch (SqlException ex)
				{
					str.Rollback();
					msgErr = Resources.MSG_ERR_004;
					return false;
				}
				finally
				{
					if (dr != null)
					{
						dr.Close();
					}
					str.Dispose();
					cn.Close();
				}
			}
		}

		public static bool CreateInputLicense(string userId, string licenseKey, string licenseTypeId, string ipAdress, out string msgErr)
		{
			using (SqlConnection cn = new SqlConnection(DBAccess.ConnectionString))
			{
				msgErr = string.Empty;
				string[] infoArr = new string[] { userId, licenseKey, ipAdress };
				string getLicenseHash = GetHashKeyQuery();

				cn.Open();
				SqlTransaction str = cn.BeginTransaction();
				SqlCommand cm = new SqlCommand(getLicenseHash, cn, str);
				cm.CommandTimeout = 120;
				cm.Parameters.Add("@hashkey", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(string.Join("", infoArr)));
				SqlDataReader dr = cm.ExecuteReader();

				try
				{
					#region Check exists
					if (dr.Read())
					{
						msgErr = Resources.MSG_ERR_009;
						return false;
					}
					dr.Close();
					#endregion

					#region Create license
					int dueDays = GetDueDaysByLicenseTypeId(licenseTypeId, cn, str, out msgErr);
					if (!string.IsNullOrEmpty(msgErr))
					{
						return false;
					}

					string createLicenseQuery = CreateLicenseQuery();
					cm = new SqlCommand(createLicenseQuery, cn, str);
					cm.CommandTimeout = 120;
					cm.Parameters.Add("@licenseKey", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(licenseKey);
					cm.Parameters.Add("@licenseType", SqlDbType.NVarChar, 10).Value = DBValueConvert.ToNvarchar(licenseTypeId);
					cm.Parameters.Add("@isUsed", SqlDbType.NVarChar, 1).Value = DBValueConvert.ToNvarchar(true);
					cm.Parameters.Add("@usedStartDate", SqlDbType.NVarChar, 30).Value = DateTime.Now;
					cm.Parameters.Add("@usedEndDate", SqlDbType.NVarChar, 30).Value = DateTimeUtil.AddDateTime(dueDays);
					cm.Parameters.Add("@isExpired", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(false);

					cm.ExecuteNonQuery();
					#endregion

					#region Create user-license mapping
					string createUserLicenseMapppingQuery = CreateUserLicenseMappingQuery();
					cm = new SqlCommand(createUserLicenseMapppingQuery, cn, str);
					cm.CommandTimeout = 120;
					cm.Parameters.Add("@userIdHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(userId));
					cm.Parameters.Add("@licenseKeyHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(licenseKey));
					// Hardcode
					cm.Parameters.Add("@hashKey", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(string.Join("", infoArr)));

					cm.ExecuteNonQuery();
					#endregion
					return true;
				}
				catch (SqlException ex)
				{
					str.Rollback();
					msgErr = Resources.MSG_ERR_011;
					return false;
				}
				finally
				{
					if (dr != null)
					{
						dr.Close();
					}
					str.Dispose();
					cn.Close();
				}
			}
		}

		public static bool CreateBuyLicense(string userId, string licenseTypeId, ConfigObjectDAO configObjectDAO, string ipAdress, out int dueDays, out string msgErr)
		{
			using (SqlConnection cn = new SqlConnection(DBAccess.ConnectionString))
			{
				msgErr = string.Empty;
				dueDays = -1;
				cn.Open();
				SqlTransaction str = cn.BeginTransaction();
				SqlCommand cm;

				try
				{
					#region Create license
					dueDays = GetDueDaysByLicenseTypeId(licenseTypeId, cn, str, out msgErr);
					if (!string.IsNullOrEmpty(msgErr))
					{
						return false;
					}

					string licenseKeyGen = GenerateLicenseKey(configObjectDAO);

					string createLicenseQuery = CreateLicenseQuery();
					cm = new SqlCommand(createLicenseQuery, cn, str);
					cm.CommandTimeout = 120;
					cm.Parameters.Add("@licenseKey", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(licenseKeyGen);
					cm.Parameters.Add("@licenseType", SqlDbType.NVarChar, 10).Value = DBValueConvert.ToNvarchar(licenseTypeId);
					cm.Parameters.Add("@isUsed", SqlDbType.NVarChar, 1).Value = DBValueConvert.ToNvarchar(true);
					cm.Parameters.Add("@usedStartDate", SqlDbType.DateTime).Value = DateTime.Now;
					cm.Parameters.Add("@usedEndDate", SqlDbType.DateTime).Value = DateTimeUtil.AddDateTime(dueDays);
					cm.Parameters.Add("@isExpired", SqlDbType.NVarChar, 1).Value = DBValueConvert.ToNvarchar(false);

					cm.ExecuteNonQuery();
					#endregion

					#region Create user-license mapping
					// Delete old data mapping
					string userIdHash = EncryptUtil.EncryptASCIIStringToBase64(userId);
					string deleteUserLicenseMapppingQuery = DeleteUserLicenseMappingQuery();
					cm = new SqlCommand(deleteUserLicenseMapppingQuery, cn, str);
					cm.Parameters.Add("@userIdHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(userIdHash);
					cm.CommandTimeout = 120;

					cm.ExecuteNonQuery();

					// Insert new data mapping
					string createUserLicenseMapppingQuery = CreateUserLicenseMappingQuery();
					cm = new SqlCommand(createUserLicenseMapppingQuery, cn, str);
					string[] infoArr = new string[] { userId, licenseKeyGen, ipAdress };
					cm.CommandTimeout = 120;
					cm.Parameters.Add("@userIdHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(userIdHash);
					cm.Parameters.Add("@licenseKeyHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(licenseKeyGen));
					// Hardcode
					cm.Parameters.Add("@hashKey", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(string.Join("", infoArr)));

					cm.ExecuteNonQuery();
					#endregion
					str.Commit();
					return true;
				}
				catch (SqlException ex)
				{
					str.Rollback();
					msgErr = Resources.MSG_ERR_011;
					return false;
				}
				finally
				{
					str.Dispose();
					cn.Close();
				}
			}
		}

		public static int CheckInputTimesAndLock(string userId, string ipAdress, int maxInputTimes, ActionTypeEnum? actionType, out bool isLocked, out string msgErr)
        {
			using (SqlConnection cn = new SqlConnection(DBAccess.ConnectionString))
			{
				isLocked = false;
				msgErr = string.Empty;
				cn.Open();
				SqlTransaction str = cn.BeginTransaction();
				int currentInputTimes = GetCurrentInputTimes(userId, ipAdress, cn, str, out msgErr);
				if (currentInputTimes == maxInputTimes)
                {
					isLocked = true;
					msgErr = Resources.MSG_LIC_INFO_003;
					return -1;
				}
				if (actionType == ActionTypeEnum.ChangeInput)
                {
					try
					{
						if (currentInputTimes == 0)
						{
							string createInputLicense = CreateInputLicenseQuery();
							SqlCommand cm = new SqlCommand(createInputLicense, cn, str);
							cm.CommandTimeout = 120;
							cm.Parameters.Add("@userId", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(userId);
							cm.Parameters.Add("@ipAdress", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(ipAdress);
							cm.Parameters.Add("@inputTimes", SqlDbType.Int).Value = 1;
							cm.Parameters.Add("@isLocked", SqlDbType.NVarChar, 1).Value = "1";

							cm.ExecuteNonQuery();
						}
						else
						{
							string updateInputLicense = UpdateInputTimesAndLockedQuery();
							SqlCommand cm = new SqlCommand(updateInputLicense, cn, str);
							cm.CommandTimeout = 120;
							currentInputTimes += 1;
							cm.Parameters.Add("@userId", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(userId);
							cm.Parameters.Add("@ipAdress", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(ipAdress);
							cm.Parameters.Add("@inputTimes", SqlDbType.Int).Value = currentInputTimes;
							cm.Parameters.Add("@isLocked", SqlDbType.NVarChar, 1).Value = currentInputTimes == maxInputTimes ? "1" : "0";

							cm.ExecuteNonQuery();
							msgErr = Resources.MSG_LIC_INFO_003;
							isLocked = true;
							return -1;
						}
						str.Commit();
					}
					catch (SqlException)
					{
						msgErr = Resources.MSG_ERR_012;
						return -1;
					}
                    finally
                    {
						str.Dispose();
						cn.Close();
                    }
				}
				return currentInputTimes;
			}
		}

		public static int GetDueDaysByLicenseTypeId(string licenseType, SqlConnection cn, SqlTransaction str, out string msgErr)
		{
			string getDueDaysByLicenseTypeIdQuery = GetLicenseDueDaysQuery();
			int value = -1;
			msgErr = string.Empty;
			SqlCommand cm = new SqlCommand(getDueDaysByLicenseTypeIdQuery, cn, str);
			cm.CommandTimeout = 120;
			cm.Parameters.Add("@licenseTypeID", SqlDbType.NVarChar, 10).Value = DBValueConvert.ToNvarchar(licenseType);
			try
			{
				SqlDataReader dr = cm.ExecuteReader();
				if (dr.Read())
				{
					value = int.Parse(dr["LICENSE_DUE_DAYS"].ToString());
				}
				dr.Close();
			}
			catch (SqlException)
			{
				msgErr = Resources.MSG_ERR_010;
				return value;
			}
			return value;
		}

		public static int GetCurrentInputTimes(string userId, string ipAdress, SqlConnection cn, SqlTransaction str, out string msgErr)
        {
			string getInputTimes = GetInputTimesByUserAndIpQuery();
			int value = 0;
			msgErr = string.Empty;
			SqlCommand cm = new SqlCommand(getInputTimes, cn, str);
			cm.CommandTimeout = 120;
			cm.Parameters.Add("@userId", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(userId);
			cm.Parameters.Add("@ipAdress", SqlDbType.NVarChar, 1).Value = DBValueConvert.ToNvarchar(ipAdress);
			try
			{
				SqlDataReader dr = cm.ExecuteReader();
				if (dr.Read())
				{
					value = int.Parse(dr["INPUT_TIMES"].ToString());
				}
				dr.Close();
			}
			catch (SqlException)
			{
				msgErr = Resources.MSG_ERR_011;
				return -1;
			}
			return value;
		}
		#endregion


		#region Query
		private static string LoginQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT USER_ID, IS_TRIAL_MODE FROM ");
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
			sb.Append(DBAccess.DBCommonSchema).Append(".[LICENSE_TYPE] ");
			sb.Append("WHERE IS_TRIAL = @isTrial");
			return sb.ToString();
		}

		private static string GetLicenseTypeForDropdownQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT LICENSE_TYPE_ID, DESCRIPTION FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[LICENSE_TYPE] ");
			sb.Append("WHERE IS_TRIAL = '0' ");
			sb.Append("ORDER BY LICENSE_TYPE_ID ASC");
			return sb.ToString();
		}

		private static string GetHashKeyQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT HASH_KEY FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[USER_LICENSE_MAPPING] ");
			sb.Append("WHERE HASH_KEY = @hashkey");
			return sb.ToString();
		}

		private static string CheckUserExistsQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT USER_ID FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[USER_INFORMATION] ");
			sb.Append("WHERE USER_ID = @userid");
			return sb.ToString();
		}

		private static string GetConfigQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT TRIAL_DUE_DAYS, CONFIG_NUM_DIGIT_PER_UNIT, CONFIG_NUM_UNIT, CONFIG_MAX_INPUT_TIMES FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[CONFIG] ");
			sb.Append("WHERE CONFIG_ID = @configID");
			return sb.ToString();
		}

		private static string GetLicenseDueDaysQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT LICENSE_DUE_DAYS FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[LICENSE_TYPE] ");
			sb.Append("WHERE LICENSE_TYPE_ID = @licenseTypeID");
			return sb.ToString();
		}

		private static string GetInputTimesByUserAndIpQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT INPUT_TIMES FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[INPUT_TIMES] ");
			sb.Append("WHERE USER_ID = @userId AND IP_ADDRESS = @ipAdress");
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

		private static string CreateInputLicenseQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("INSERT INTO ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[INPUT_LICENSE] ");
			sb.Append("VALUES (@userId, @ipAdress, @inputTimes, @isLocked)");
			return sb.ToString();
		}

		private static string UpdateInputTimesAndLockedQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("UPDATE TABLE ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[INPUT_LICENSE] ");
			sb.Append("SET ");
			sb.Append("INPUT_TIMES = @inputTimes, IS_LOCKED = @isLock ");
			sb.Append("WHERE USER_ID = @userId AND IP_ADDRESS = @ipAdress");
			return sb.ToString();
		}

		private static string DeleteUserLicenseMappingQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("DELETE FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[USER_LICENSE_MAPPING] ");
			sb.Append("WHERE USER_ID_HASH = @userIdHash");
			return sb.ToString();
		}

		#endregion

		#region Private method
		private static string GenerateLicenseKey(ConfigObjectDAO configObjectDAO)
		{
			StringBuilder sb = new StringBuilder();
			string randItemStr;
			for (int i = 0; i < configObjectDAO.ConfigNumberOfUnit; i++)
			{
				randItemStr = string.Empty;
				Thread.Sleep(100);
				randItemStr = StringUtil.CreateRandomString(configObjectDAO.ConfigNumberDigitPerUnit);
				sb.Append(randItemStr);
				if (i < configObjectDAO.ConfigNumberOfUnit - 1)
				{
					sb.Append(configObjectDAO.ConfigLicenseSuffix);
				}
			}

			return sb.ToString();
		}

		private static DataTable MakeDataTableLogin()
		{
			DataTable dt = new DataTable();

			DataColumn dCol = new DataColumn();
			dCol.DataType = Type.GetType("System.String");
			dCol.ColumnName = "USER_ID";
			dCol.MaxLength = 30;
			dt.Columns.Add(dCol);

			dCol = new DataColumn();
			dCol.DataType = Type.GetType("System.String");
			dCol.ColumnName = "IS_TRIAL_MODE";
			dCol.MaxLength = 1;
			dt.Columns.Add(dCol);

			dCol = new DataColumn();
			dCol.DataType = Type.GetType("System.String");
			dCol.ColumnName = "LICENSE_KEY";
			dCol.MaxLength = 30;
			dt.Columns.Add(dCol);

			dCol = new DataColumn();
			dCol.DataType = Type.GetType("System.String");
			dCol.ColumnName = "IS_EXPIRED";
			dCol.MaxLength = 1;
			dt.Columns.Add(dCol);

			dCol = new DataColumn();
			dCol.DataType = Type.GetType("System.DateTime");
			dCol.ColumnName = "USED_END_DATE";
			dt.Columns.Add(dCol);

			return dt;
		}
		#endregion
	}
}
