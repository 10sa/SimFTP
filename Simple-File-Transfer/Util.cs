using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Collections;

using System.Configuration;
using System.Configuration.Assemblies;
using System.Collections.Specialized;


namespace Simple_File_Transfer
{
	public static class Util
	{
		#region AttachByteArray Main/Sub Methods
		public static byte[] AttachByteArray(params byte[][] arrays)
		{
			byte[] buffer = new byte[GetArraySize(arrays)];

			long bufferPointer = 0;
			foreach (var array in arrays)
			{
				Array.Copy(array, 0, buffer, bufferPointer, array.LongLength);
				bufferPointer += array.LongLength;
			}

			return buffer;
		}

		private static long GetArraySize(params Array[] arrays)
		{
			long size = 0;
			foreach (var array in arrays)
				size += array.LongLength;

			return size;
		}
		#endregion

		public static byte[] GetHashValue(byte[] orignal)
		{
			using (SHA256CryptoServiceProvider csp = new SHA256CryptoServiceProvider())
			{
				return csp.ComputeHash(orignal);
			}
		}

		public static string GetHashedString(string data)
		{
			return Encoding.UTF8.GetString(GetHashValue(Encoding.UTF8.GetBytes(data)));
		}

		#region Configuration Code
		// Config IO Buffer //
		private static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
		private static KeyValueConfigurationCollection dataCollection = config.AppSettings.Settings;

		private const string AccountDataTag = "Account-";

		public static string GetConfigData(string key)
		{
			return ConfigurationManager.AppSettings[key];
		}

		public static void SetConfigData(string key, string value)
		{
			dataCollection.Remove(key);
			dataCollection.Add(key, value);

			SaveConfigData();
			return;
		}

		public static void RemoveConfigData(string key)
		{
			try
			{
				dataCollection.Remove(key);
				SaveConfigData();
			}
			catch(Exception) { throw; }

			return;
		}

		public static string GetAccountPassword(string username)
		{
			return ConfigurationManager.AppSettings[AccountDataTag + username];
		}

		public static void RefreshAccountInfo(string username, string password)
		{
			dataCollection.Remove(AccountDataTag + username);
			dataCollection.Add(AccountDataTag + username, GetHashedString(password));

			SaveConfigData();
			return;
		}

		private static void SaveConfigData()
		{
			config.Save(ConfigurationSaveMode.Modified);
			ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);

			return;
		}
		#endregion

		public static void WriteFile(byte[] file, string fileName)
		{
			using (FileStream fileStream = new FileStream(fileName, FileMode.CreateNew))
			{
				fileStream.Write(file, 0, file.Length);
				fileStream.FlushAsync();

				return;
			}
		}
	}
}