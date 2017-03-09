using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Simple_File_Transfer.Config
{
	public class AccountConfig : ConfigManager
	{
		public AccountConfig() : base("AccountConfig.cfg") { }
		private const string AccountTag = "Account - ";

		public override void AddConfigTable(string key, string value)
		{
			base.AddConfigTable(AccountTag + key, Util.GetHashedString(value));
		}

		public override string GetConfigTable(string key)
		{
			return base.GetConfigTable(key).Substring(AccountTag.Length + 1);
		}

		// Do Nothing! //
		protected override void InitializeConfig() { }
	}
}
