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
		public static string EncryptASCIIStringToBase64(string input)
		{
			try
			{
				byte[] byteEncodeArray = Encoding.ASCII.GetBytes(input);
				string value = Convert.ToBase64String(byteEncodeArray);
				return value;
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public static string DecryptASCIIStringFromBase64(string input)
		{
			try
			{
				string padded = input.PadRight(input.Length + (4 - input.Length % 4) % 4, '=');
				byte[] byteDecodeArray = Convert.FromBase64String(padded);
				string value = Encoding.ASCII.GetString(byteDecodeArray, 0, byteDecodeArray.Length);
				return value;
			}
			catch (Exception ex)
			{
				return string.Empty;
			}
		}
	}
}
