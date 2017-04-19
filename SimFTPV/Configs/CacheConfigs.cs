using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimFTP.Config;

namespace SimFTPV.Configs
{
	public class CacheConfigs : ConfigManager
	{
		protected override void InitializeConfig()
		{
			AddConfigTable("Count", "0");
			AddConfigTable("0", "");
			AddConfigTable("1", "");
			AddConfigTable("2", "");
			AddConfigTable("3", "");
			AddConfigTable("4", "");
		}

		public CacheConfigs() : base("AddressCache.cache") { }

		public override void AddConfigTable(string key, string value)
		{
			int Count = int.Parse(GetConfigTable("Count"));

			if(Count < 4)
				Count++;
			else
				Count = 0;

			SetConfigTable("Count", Count.ToString());
			SetConfigTable(Count.ToString(), value);
		}
	}
}
