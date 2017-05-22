using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using SimFTP.Config;

namespace SimFTPV.Configs
{
	public class CacheConfigs : ConfigManager
	{
		private int cacheMaxCount = 4;

		public CacheConfigs() : base("AddressCache.cache") { }

		protected override void InitializeConfig()
		{
			base.AddConfigTable("Count", "0");
			AddConfigTable("0", "");
			AddConfigTable("1", "");
			AddConfigTable("2", "");
			AddConfigTable("3", "");
			AddConfigTable("4", "");
		}

		public override void AddConfigTable(string key, string value)
		{
			int Count = int.Parse(GetConfigTable("Count"));

			if(Count < cacheMaxCount)
				Count++;
			else
				Count = 0;

			SetConfigTable("Count", Count.ToString());
			SetConfigTable(Count.ToString(), value);
		}

		public string[] GetAllCacheValue()
		{
			List<string> tmp = new List<string>();
			foreach (var data in ConfigTable)
			{
				if(data.Key != "Count")
					tmp.Add(data.Value);
			}

			return tmp.ToArray();
		}

		public void DeleteCache()
		{
			File.Delete("AddressCache.cache");
		}
	}
}
