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
		public static bool LoginCheck(string useID, string password, string ipAdress, out DataTable dt, out string msgErr)
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
						dt = MakeSessionDataTable();

						dt.BeginInit();
						DataRow dRow = dt.NewRow();
						dRow["USER_ID"] = dr["USER_ID"].ToString();
						dRow["IS_LOCKED_IP"] = DBNull.Value;
						dRow["LICENSE_KEY"] = DBNull.Value;
						dRow["IS_EXPIRED"] = DBNull.Value;
						dRow["USED_END_DATE"] = DBNull.Value;
						dRow["IS_TRIAL_MODE"] = DBNull.Value;
						dt.Rows.Add(dRow);
						dt.EndInit();
						dt.AcceptChanges();
						dr.Close();

						string getLicenseHasQuery = GetLicenseMappingByUserIdQuery();
						cm = new SqlCommand(getLicenseHasQuery, cn);
						cm.CommandTimeout = 120;
						cm.Parameters.Add("@useridHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(useID));
						SqlDataReader dr2 = cm.ExecuteReader();
						if (dr2.Read())
						{
							string licenseHash = dr2["LICENSE_KEY_HASH"].ToString();
							string userIdHash = dr2["USER_ID_HASH"].ToString();
							string ipAdressHash = dr2["IP_HASH"].ToString();
							if (EncryptUtil.DecryptASCIIStringFromBase64(userIdHash) != useID || EncryptUtil.DecryptASCIIStringFromBase64(ipAdressHash) != ipAdress)
                            {
								dt.BeginLoadData();
								dt.Rows[0]["IS_LOCKED_IP"] = StringUtil.ToBoolValue("1");
								dt.EndLoadData();
								dt.AcceptChanges();
								dr2.Close();
								return true;
							}
                            else
                            {
								dr2.Close();
							}

							DataTable dtLicInfo = GetLicenseInfo(EncryptUtil.DecryptASCIIStringFromBase64(licenseHash), cn, null, out msgErr);
							if (dtLicInfo != null && dtLicInfo.Rows.Count > 0)
							{
								dt.BeginLoadData();
								dt.Rows[0]["LICENSE_KEY"] = dtLicInfo.Rows[0]["LICENSE_KEY"].ToString();
								dt.Rows[0]["IS_EXPIRED"] = StringUtil.ToBoolValue(dtLicInfo.Rows[0]["IS_EXPIRED"].ToString());
								dt.Rows[0]["USED_END_DATE"] = (DateTime)dtLicInfo.Rows[0]["USED_END_DATE"];
								dt.Rows[0]["IS_TRIAL_MODE"] = StringUtil.ToBoolValue(dtLicInfo.Rows[0]["IS_TRIAL"].ToString());
								dt.EndLoadData();
								dt.AcceptChanges();
							}
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

		public static DataTable GetLicenseTypeId(SqlConnection cn, SqlTransaction str)
        {
			DataTable dt = new DataTable();
			string GetLicenseTypeIDQuery = GetLicenseTypeByTrialQuery();
			SqlCommand cm = new SqlCommand(GetLicenseTypeIDQuery, cn, str);
			cm.CommandTimeout = 120;
			cm.Parameters.Add("@isTrial", SqlDbType.NVarChar, 1).Value = DBValueConvert.ToNvarchar(true);
			SqlDataAdapter da = new SqlDataAdapter(cm);
			try
			{
				da.Fill(dt);
				return dt;
			}
			catch (SqlException)
			{
				return null;
			}
			finally
			{
				da.Dispose();
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

		public static bool CreateAccount(string useID, string password, string userIp, ConfigObjectDAO configObjectDAO, out string msgErr)
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
					DataTable dtTrialLic = GetLicenseTypeId(cn, str);

					string createLicenseQuery = CreateLicenseQuery();
					cm = new SqlCommand(createLicenseQuery, cn, str);
					cm.CommandTimeout = 120;
					cm.Parameters.Add("@licenseKey", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(licenseKeyGen);
					cm.Parameters.Add("@licenseType", SqlDbType.NVarChar, 10).Value = DBValueConvert.ToNvarchar(dtTrialLic.Rows[0]["LICENSE_TYPE_ID"].ToString());
					cm.Parameters.Add("@isUsed", SqlDbType.NVarChar, 1).Value = DBValueConvert.ToNvarchar(true);
					cm.Parameters.Add("@usedStartDate", SqlDbType.DateTime).Value = DateTime.Now;
					cm.Parameters.Add("@usedEndDate", SqlDbType.DateTime).Value = DateTimeUtil.AddDateTime((int)dtTrialLic.Rows[0]["LICENSE_DUE_DAYS"]);
					cm.Parameters.Add("@isExpired", SqlDbType.NVarChar, 1).Value = DBValueConvert.ToNvarchar(false);

					cm.ExecuteNonQuery();
					#endregion

					#region Create user-license mapping
					string createUserLicenseMapppingQuery = CreateUserLicenseMappingQuery();
					cm = new SqlCommand(createUserLicenseMapppingQuery, cn, str);
					cm.CommandTimeout = 120;
					cm.Parameters.Add("@userIdHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(useID));
					cm.Parameters.Add("@licenseKeyHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(licenseKeyGen));
					// Hardcode
					cm.Parameters.Add("@ipHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(userIp));

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

		public static bool CreateInputLicense(string userId, string userIp, string licenseKey, out DataTable dt, out string msgErr)
		{
			using (SqlConnection cn = new SqlConnection(DBAccess.ConnectionString))
			{
				dt = MakeSessionDataTable();
				msgErr = string.Empty;
				cn.Open();
				SqlTransaction str = cn.BeginTransaction();
				SqlCommand cm;

				try
				{
					#region Check exists License
					string checkExistLicense = CheckExistLicenseQuery();
					cm = new SqlCommand(checkExistLicense, cn, str);
					cm.CommandTimeout = 120;
					cm.Parameters.Add("@licenseKey", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(licenseKey);

					int count = (int) cm.ExecuteScalar();
					if (count == 0)
					{
						msgErr = Resources.MSG_ERR_014;
						return false;
					}
                    #endregion

                    #region Check exists User-License Mapping
                    string checkExistUserLicense = CheckExistUserLicenseMappingByLicenseQuery();
					string licenseKeyHash = EncryptUtil.EncryptASCIIStringToBase64(licenseKey);
					cm = new SqlCommand(checkExistUserLicense, cn, str);
					cm.CommandTimeout = 120;
					cm.Parameters.Add("@licenseKeyHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(licenseKeyHash);
					count = (int)cm.ExecuteScalar();
					if (count > 0)
					{
						msgErr = Resources.MSG_ERR_015;
						return false;
					}
					#endregion

					#region Update license used
					string licenseTypeId = GetLicenseTypeId(licenseKey, cn, str, out msgErr);
					if (string.IsNullOrEmpty(licenseTypeId))
					{
						return false;
					}

					int licenseDueDays = GetDueDaysByLicenseTypeId(licenseTypeId, cn, str, out msgErr);
					if (licenseDueDays == -1)
					{
						return false;
					}

					string updateLicenseUsedQuery = UpdateLicenseUsedQuery();
					cm = new SqlCommand(updateLicenseUsedQuery, cn, str);
					cm.Parameters.Add("@isUsed", SqlDbType.NVarChar, 1).Value = DBValueConvert.ToNvarchar(true);
					cm.Parameters.Add("@usedStartDate", SqlDbType.DateTime).Value = DateTime.Now;
					cm.Parameters.Add("@usedEndDate", SqlDbType.DateTime).Value = DateTimeUtil.AddDateTime(licenseDueDays);
					cm.Parameters.Add("@licenseKey", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(licenseKey);

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

					string createUserLicenseMapppingQuery = CreateUserLicenseMappingQuery();
					cm = new SqlCommand(createUserLicenseMapppingQuery, cn, str);
					cm.CommandTimeout = 120;
					cm.Parameters.Add("@userIdHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(userId));
					cm.Parameters.Add("@licenseKeyHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(licenseKeyHash);
					// Hardcode
					cm.Parameters.Add("@ipHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(userIp));

					cm.ExecuteNonQuery();
					#endregion
					str.Commit();
					DataTable dtLicInfo = GetLicenseInfo(licenseKey, cn, null, out msgErr);
					if (dtLicInfo != null && dtLicInfo.Rows.Count > 0)
					{
						dt.BeginInit();
						DataRow dRow = dt.NewRow();
						dRow["USER_ID"] = userId;
						dRow["IS_TRIAL_MODE"] = StringUtil.ToBoolValue(dtLicInfo.Rows[0]["IS_TRIAL"].ToString());
						dRow["IS_LOCKED_IP"] = false;
						dRow["LICENSE_KEY"] = dtLicInfo.Rows[0]["LICENSE_KEY"].ToString();
						dRow["IS_EXPIRED"] = StringUtil.ToBoolValue(dtLicInfo.Rows[0]["IS_EXPIRED"].ToString());
						dRow["USED_END_DATE"] = (DateTime)dtLicInfo.Rows[0]["USED_END_DATE"];
						dt.Rows.Add(dRow);
						dt.EndInit();
						dt.AcceptChanges();
					}

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

		public static bool CreateBuyLicense(string userId, string licenseTypeId, ConfigObjectDAO configObjectDAO, string ipAdress, out int dueDays, out string licenseKeyGen, out string msgErr)
		{
			using (SqlConnection cn = new SqlConnection(DBAccess.ConnectionString))
			{
				licenseKeyGen = null;
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

					licenseKeyGen = GenerateLicenseKey(configObjectDAO);

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
					cm.Parameters.Add("@ipHash", SqlDbType.NVarChar, 200).Value = DBValueConvert.ToNvarchar(EncryptUtil.EncryptASCIIStringToBase64(ipAdress));

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

		public static bool CheckInputTimesAndLock(string userId, string ipAdress, int maxInputTimes, ActionTypeEnum? actionType, out string msgErr)
        {
			using (SqlConnection cn = new SqlConnection(DBAccess.ConnectionString))
			{
				msgErr = string.Empty;
				cn.Open();
				SqlTransaction str = cn.BeginTransaction();
				#region Check user input
				int userInputTimes = 0;
				bool isLocked = false;
				DataTable dtUserInp = IsUserInputLocked(userId, cn, str, out msgErr);
				if (dtUserInp != null && dtUserInp.Rows.Count > 0)
				{
					bool canParse = int.TryParse(dtUserInp.Rows[0]["INPUT_TIMES"].ToString(), out userInputTimes);
					if (!canParse)
					{
						msgErr = Resources.MSG_ERR_012;
						return false;
					}
					if (dtUserInp.Rows[0]["IS_LOCKED"] != null)
					{
						isLocked = (bool)StringUtil.ToBoolValue(dtUserInp.Rows[0]["IS_LOCKED"].ToString());
						if (isLocked) return true;
					}
				}
				#endregion

				#region Check ip input
				int ipInputTimes = 0;
				DataTable dtIpInp = IsIpInputLocked(ipAdress, cn, str, out msgErr);
				if (dtIpInp != null && dtIpInp.Rows.Count > 0)
				{
					bool canParse = int.TryParse(dtIpInp.Rows[0]["INPUT_TIMES"].ToString(), out ipInputTimes);
					if (!canParse)
					{
						msgErr = Resources.MSG_ERR_012;
						return false;
					}
					if (dtIpInp.Rows[0]["IS_LOCKED"] != null)
					{
						isLocked = (bool)StringUtil.ToBoolValue(dtIpInp.Rows[0]["IS_LOCKED"].ToString());
						if (isLocked) return true;
					}
				}
				#endregion

				if (actionType == ActionTypeEnum.ChangeInput)
                {
					try
					{
						#region User input
						if (userInputTimes == 0)
						{
							string createInputLicense = CreateUserInputLicenseQuery();
							SqlCommand cm = new SqlCommand(createInputLicense, cn, str);
							cm.CommandTimeout = 120;
							cm.Parameters.Add("@userId", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(userId);
							cm.Parameters.Add("@inputTimes", SqlDbType.Int).Value = 1;
							cm.Parameters.Add("@isLocked", SqlDbType.NVarChar, 1).Value = "0";

							cm.ExecuteNonQuery();
						}
						else
						{
							string updateInputLicense = UpdateUserInputQuery();
							SqlCommand cm = new SqlCommand(updateInputLicense, cn, str);
							cm.CommandTimeout = 120;
							userInputTimes += 1;
							cm.Parameters.Add("@userId", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(userId);
							cm.Parameters.Add("@inputTimes", SqlDbType.Int).Value = userInputTimes;
							cm.Parameters.Add("@isLocked", SqlDbType.NVarChar, 1).Value = DBValueConvert.ToNvarchar(userInputTimes > maxInputTimes);

							cm.ExecuteNonQuery();
						}
						#endregion

						#region Ip input
						if (ipInputTimes == 0)
						{
							string createInputLicense = CreateIpInputLicenseQuery();
							SqlCommand cm = new SqlCommand(createInputLicense, cn, str);
							cm.CommandTimeout = 120;
							cm.Parameters.Add("@ipAdress", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(ipAdress);
							cm.Parameters.Add("@inputTimes", SqlDbType.Int).Value = 1;
							cm.Parameters.Add("@isLocked", SqlDbType.NVarChar, 1).Value = "0";

							cm.ExecuteNonQuery();
						}
						else
						{
							string updateInputLicense = UpdateIpInputQuery();
							SqlCommand cm = new SqlCommand(updateInputLicense, cn, str);
							cm.CommandTimeout = 120;
							ipInputTimes += 1;
							cm.Parameters.Add("@ipAdress", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(ipAdress);
							cm.Parameters.Add("@inputTimes", SqlDbType.Int).Value = userInputTimes;
							cm.Parameters.Add("@isLocked", SqlDbType.NVarChar, 1).Value = DBValueConvert.ToNvarchar(ipInputTimes > maxInputTimes);

							cm.ExecuteNonQuery();

						}
						#endregion
						str.Commit();

						// Check if max after insert/update
						return (userInputTimes > maxInputTimes || ipInputTimes > maxInputTimes);
					}
					catch (SqlException ex)
					{
						str.Rollback();
						msgErr = Resources.MSG_ERR_012;
						return false;
					}
                    finally
                    {
						str.Dispose();
						cn.Close();
                    }
				}
				return isLocked;
			}
		}

		public static DataTable IsIpInputLocked(string ipAdress, SqlConnection cn, SqlTransaction str, out string msgErr)
        {
			//ipInputTimes = -1;
			//bool isLocked = true;
			DataTable dt = new DataTable();
			msgErr = string.Empty;
			string getInputTimes = GetIpInputTimesQuery();
			SqlCommand cm = new SqlCommand(getInputTimes, cn, str);
			cm.CommandTimeout = 120;
			cm.Parameters.Add("@ipAdress", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(ipAdress);
			//SqlDataReader dr = cm.ExecuteReader();
			SqlDataAdapter da = new SqlDataAdapter(cm);
			try
			{
				da.Fill(dt);
			}
			catch (SqlException)
			{
				msgErr = Resources.MSG_ERR_011;
				return null;
			}
			finally
			{
				da.Dispose();
			}
			return dt;
		}

		public static DataTable IsUserInputLocked(string userId, SqlConnection cn, SqlTransaction str, out string msgErr)
		{
			DataTable dt = new DataTable();
			msgErr = string.Empty;
			string getInputTimes = GetUserInputTimesQuery();
			SqlCommand cm = new SqlCommand(getInputTimes, cn, str);
			cm.CommandTimeout = 120;
			cm.Parameters.Add("@userId", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(userId);
			//SqlDataReader dr = cm.ExecuteReader();
			SqlDataAdapter da = new SqlDataAdapter(cm);
			try
			{
				da.Fill(dt);
			}
			catch (SqlException)
			{
				msgErr = Resources.MSG_ERR_011;
				return null;
			}
			finally
			{
				da.Dispose();
			}
			return dt;
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

		public static string GetLicenseTypeId(string licenseKey, SqlConnection cn, SqlTransaction str, out string msgErr)
		{
			string getLicenseTypeByLicenseQuery = GetLicenseTypeByLicenseQuery();
			string value = string.Empty;
			msgErr = string.Empty;
			SqlCommand cm = new SqlCommand(getLicenseTypeByLicenseQuery, cn, str);
			cm.CommandTimeout = 120;
			cm.Parameters.Add("@licenseKey", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(licenseKey);
			try
			{
				SqlDataReader dr = cm.ExecuteReader();
				if (dr.Read())
				{
					value = dr["LICENSE_TYPE_ID"].ToString();
				}
				dr.Close();
			}
			catch (SqlException)
			{
				msgErr = Resources.MSG_ERR_010;
				return null;
			}
			return value;
		}

		public static DataTable GetLicenseInfo(string licenseKey, SqlConnection cn, SqlTransaction str, out string msgErr)
		{
			DataTable dt = new DataTable();
			string getDueDaysByLicenseTypeIdQuery = GetLicenseInfoQuery();
			msgErr = string.Empty;
			SqlCommand cm;
			if (str != null)
			{
				cm = new SqlCommand(getDueDaysByLicenseTypeIdQuery, cn, str);
			}
			else
			{
				cm = new SqlCommand(getDueDaysByLicenseTypeIdQuery, cn);
			}
			cm.CommandTimeout = 120;
			cm.Parameters.Add("@licenseKey", SqlDbType.NVarChar, 30).Value = DBValueConvert.ToNvarchar(licenseKey);
			SqlDataAdapter da = new SqlDataAdapter(cm);
			try
			{
				da.Fill(dt);
			}
			catch (SqlException)
			{
				msgErr = Resources.MSG_ERR_010;
				return null;
			}
			finally
			{
				da.Dispose();
			}
			return dt;
		}
		#endregion


		#region Query
		private static string LoginQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT USER_ID FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[USER_INFORMATION] ");
			sb.Append("WHERE USER_ID = @userid AND PASSWORD = @password");
			return sb.ToString();
		}

		private static string GetLicenseMappingByUserIdQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT USER_ID_HASH, LICENSE_KEY_HASH, IP_HASH FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[USER_LICENSE_MAPPING] ");
			sb.Append("WHERE USER_ID_HASH = @useridHash");
			return sb.ToString();
		}

		private static string GetLicenseInfoQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT LICENSE_KEY, IS_EXPIRED, USED_END_DATE, IS_TRIAL FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[LICENSE] ");
			sb.Append("INNER JOIN ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[LICENSE_TYPE] ");
			sb.Append("ON [LICENSE].LICENSE_TYPE_ID = [LICENSE_TYPE].LICENSE_TYPE_ID ");
			sb.Append("WHERE LICENSE_KEY = @licenseKey");
			return sb.ToString();
		}

		private static string GetLicenseTypeByTrialQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT LICENSE_TYPE_ID, LICENSE_DUE_DAYS FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[LICENSE_TYPE] ");
			sb.Append("WHERE IS_TRIAL = @isTrial");
			return sb.ToString();
		}

		private static string GetLicenseTypeByLicenseQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT LICENSE_TYPE_ID FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[LICENSE] ");
			sb.Append("WHERE LICENSE_KEY = @licenseKey");
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

		private static string CheckExistUserLicenseMappingByLicenseQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT COUNT(*) FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[USER_LICENSE_MAPPING] ");
			sb.Append("WHERE LICENSE_KEY_HASH = @licenseKeyHash");
			return sb.ToString();
		}

		private static string CheckExistLicenseQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT COUNT(*) FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[LICENSE] ");
			sb.Append("WHERE LICENSE_KEY = @licenseKey AND IS_USED = '0'");
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
			sb.Append("SELECT CONFIG_NUM_DIGIT_PER_UNIT, CONFIG_NUM_UNIT, CONFIG_LICENSE_SUFFIX, CONFIG_MAX_INPUT_TIMES FROM ");
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

		private static string GetUserInputTimesQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT INPUT_TIMES, IS_LOCKED FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[INPUT_USER_LICENSE] ");
			sb.Append("WHERE USER_ID = @userId");
			return sb.ToString();
		}

		private static string GetIpInputTimesQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT INPUT_TIMES, IS_LOCKED FROM ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[INPUT_IP_LICENSE] ");
			sb.Append("WHERE IP_ADDRESS = @ipAdress");
			return sb.ToString();
		}

		private static string CreateUserQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("INSERT INTO ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[USER_INFORMATION] ");
			sb.Append("VALUES (@userid, @password, @passwordhash, GETDATE())");
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
			sb.Append("VALUES (@userIdHash, @licenseKeyHash, @ipHash, GETDATE())");
			return sb.ToString();
		}

		private static string CreateUserInputLicenseQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("INSERT INTO ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[INPUT_USER_LICENSE] ");
			sb.Append("VALUES (@userId, @inputTimes, @isLocked)");
			return sb.ToString();
		}

		private static string CreateIpInputLicenseQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("INSERT INTO ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[INPUT_IP_LICENSE] ");
			sb.Append("VALUES (@ipAdress, @inputTimes, @isLocked)");
			return sb.ToString();
		}

		private static string UpdateLicenseUsedQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("UPDATE ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[LICENSE] ");
			sb.Append("SET ");
			sb.Append("IS_USED = @isUsed, USED_START_DATE = @usedStartDate, USED_END_DATE = @usedEndDate ");
			sb.Append("WHERE LICENSE_KEY = @licenseKey");
			return sb.ToString();
		}

		private static string UpdateUserInputQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("UPDATE ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[INPUT_USER_LICENSE] ");
			sb.Append("SET ");
			sb.Append("INPUT_TIMES = @inputTimes, IS_LOCKED = @isLocked ");
			sb.Append("WHERE USER_ID = @userId");
			return sb.ToString();
		}

		private static string UpdateIpInputQuery()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("UPDATE ");
			sb.Append(DBAccess.DBCommonSchema).Append(".[INPUT_IP_LICENSE] ");
			sb.Append("SET ");
			sb.Append("INPUT_TIMES = @inputTimes, IS_LOCKED = @isLocked ");
			sb.Append("WHERE IP_ADDRESS = @ipAdress");
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

		private static DataTable MakeSessionDataTable()
		{
			DataTable dt = new DataTable();

			DataColumn dCol = new DataColumn();
			dCol.DataType = Type.GetType("System.String");
			dCol.ColumnName = "USER_ID";
			dCol.MaxLength = 30;
			dt.Columns.Add(dCol);

			dCol = new DataColumn();
			dCol.DataType = Type.GetType("System.Boolean");
			dCol.ColumnName = "IS_TRIAL_MODE";
			dt.Columns.Add(dCol);

			dCol = new DataColumn();
			dCol.DataType = Type.GetType("System.Boolean");
			dCol.ColumnName = "IS_LOCKED_IP";
			dt.Columns.Add(dCol);

			dCol = new DataColumn();
			dCol.DataType = Type.GetType("System.String");
			dCol.ColumnName = "LICENSE_KEY";
			dCol.MaxLength = 30;
			dt.Columns.Add(dCol);

			dCol = new DataColumn();
			dCol.DataType = Type.GetType("System.Boolean");
			dCol.ColumnName = "IS_EXPIRED";
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
