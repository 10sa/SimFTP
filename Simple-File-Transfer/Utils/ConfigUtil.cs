using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Simple_File_Transfer.Util
{
	#region Configuration Code
	public static class ConfigUtil
	{
		private const string AccountDataTag = "Account-";

		private static Dictionary<string, string> configTable = new Dictionary<string, string> { };
		private static StreamWriter fileWriter = new StreamWriter("Config.cfg");
		private static BinaryFormatter binaryFormatter = new BinaryFormatter();

		public static void InitializeConfig()
		{
			configTable.Add("Accept_Default_Packet", bool.TrueString);
			configTable.Add("Accept_Basic_Security_Packet", bool.FalseString);
			configTable.Add("Accept_Anonymous_Login", bool.FalseString);

			SaveConfigData();
		}

		public static string GetConfigData(string key)
		{
			return configTable[key];
		}

		public static void SetConfigData(string key, string value, bool isSave = false)
		{
			configTable[key] = value;

			if (isSave)
				SaveConfigData();
		}

		public static void RemoveConfig(string key)
		{
			configTable.Remove(key);
		}

		private static void SaveConfigData()
		{
			binaryFormatter.Serialize(fileWriter.BaseStream, configTable);
		}

		private static bool IsExistsConfig()
		{
			return File.Exists("Config.cfg");
		}
	}
	#endregion
}