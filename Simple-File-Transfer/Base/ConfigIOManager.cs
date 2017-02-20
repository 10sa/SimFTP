using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Simple_File_Transfer.Base
{
	public abstract class ConfigIOManager
	{
		private BinaryFormatter binaryFormatter = new BinaryFormatter();
		private Stream fileStream;

		protected Dictionary<string, string> configTable = new Dictionary<string, string> { };

		// Not Using This Constructor. //
		private ConfigIOManager() { }

		public ConfigIOManager(string path)
		{
			if (File.Exists(path))
				fileStream = File.Open(path, FileMode.Open);			
			else
				InitializeConfig();

			LoadData();
		}

		protected abstract void InitializeConfig();

		protected void SaveData()
		{
			try
			{
				binaryFormatter.Serialize(fileStream, configTable);
				fileStream.Flush();
			}
			catch
			{
				Console.Error.WriteLine("[Error] Data Serialize Failure.");
				throw;
			}
		}

		protected void LoadData()
		{
			try
			{
				configTable = (Dictionary<string, string>)binaryFormatter.Deserialize(fileStream);
			}
			catch
			{
				InitializeConfig();

				try
				{
					configTable = (Dictionary<string, string>)binaryFormatter.Deserialize(fileStream);
				}
				catch
				{
					Console.Error.WriteLine("[Error] Data Deserialize Failure.");
					throw;
				}
			}
		}
	}
}
