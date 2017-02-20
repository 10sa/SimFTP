using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using Simple_File_Transfer.Base;

namespace Simple_File_Transfer.Utils
{
	public class AccountManager : ConfigIOManager
	{
		public AccountManager(string path) : base(path)
		{
			
		}

		protected override void InitializeConfig()
		{
			throw new NotImplementedException();
		}
	}
}
