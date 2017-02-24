using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple_File_Transfer.Util;

namespace Simple_File_Transfer.Config
{
	public class AccountManager : ConfigManager
	{
		public AccountManager(string path) : base(path) { }
		private const string AccountTag = "Account - ";

		public override void AddConfigTable(string key, string value)
		{
			base.AddConfigTable(AccountTag + key, Utils.GetHashedString(value));
		}

		public override string GetConfigTable(string key)
		{
			return base.GetConfigTable(key).Substring(AccountTag.Length + 1);
		}
	}
}
