using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SimFTP.Config
{
	public class AccountConfig : ConfigManager
	{
		public AccountConfig() : base("AccountConfig.cfg") { }

		public override void AddConfigTable(string key, string value)
		{
			base.AddConfigTable(key, Util.GetHashedString(value));
		}

		public void RemoveAccount(string key)
		{
			ConfigTable.Remove(key);
		}

		public override string GetConfigTable(string key)
		{
			try
			{
				return base.GetConfigTable(key);
			}
			catch (Exception) { return null; }
		}

		// Do Nothing! //
		protected override void InitializeConfig() { }
	}
}
