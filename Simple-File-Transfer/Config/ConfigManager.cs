using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace Simple_File_Transfer.Config
{
	public class ConfigManager : IDisposable
	{
		private BinaryFormatter binaryFormatter = new BinaryFormatter();
		private object threadLocker = new object();
		private Stream fileStream;

		protected Dictionary<string, string> configTable = new Dictionary<string, string>();
		public event Action InitializeConfig = () => { };

		// Not Using This Constructor. //
		private ConfigManager() { }

		public ConfigManager(string path)
		{
			if (File.Exists(path))
			{
				fileStream = File.Open(path, FileMode.Open);
				LoadData();
			}
			else
				InitializeConfig();
		}

		public virtual void AddConfigTable(string key, string value)
		{
			lock(threadLocker)
			{
				configTable.Add(key, value);
			}
		}

		public virtual string GetConfigTable(string key)
		{
			return configTable[key];
		}

		public void SaveData()
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

		public void LoadData()
		{
			try
			{
				configTable = (Dictionary<string, string>)binaryFormatter.Deserialize(fileStream);
			}
			catch
			{
				Console.Error.WriteLine("[Error] Data Deserialize Failure.");
				InitializeConfig();
			}
		}

		#region IDisposable Support
		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					fileStream.Dispose();
				}

				InitializeConfig = null;
				binaryFormatter = null;
				configTable = null;

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
