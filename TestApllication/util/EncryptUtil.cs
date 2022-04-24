using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TestApllication.util
{
	internal class EncryptUtil
	{
		private const int KeySizes = 256;
		private const int IvSize = 64;
		private static readonly byte[] Salt = new Guid("cf9c387f-8597-48e2-b96a-5377b91d8344").ToByteArray();
		/// <summary>
		/// Process encrypt the image by Base64 encryption
		/// </summary>
		/// <param name="path">Image path</param>
		/// <returns>Encrypted image</returns>
		public static string EncryptStringToBase64(string input)
		{
			try
			{
				using (MemoryStream sr = new MemoryStream())
				{
					var sw = new StreamWriter(sr);
					sw.Write(input);
					byte[] inArray = sr.ToArray();
					return Convert.ToBase64String(inArray);
				}
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public static string DecryptStringFromBase64(string input)
		{
			try
			{
				return Encoding.UTF8.GetString(Convert.FromBase64String(input));
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}
}
