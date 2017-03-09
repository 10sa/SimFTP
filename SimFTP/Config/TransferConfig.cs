using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_File_Transfer.Config
{
	public class TransferConfig : ConfigManager
	{
		public TransferConfig() : base("TransferConfig.cfg") { }

		protected override void InitializeConfig()
		{
			AddConfigTable("Accept_Default_Packet", bool.TrueString);
			AddConfigTable("Accept_Basic_Security_Packet", bool.FalseString);
			AddConfigTable("Accept_Anonymous_Login", bool.FalseString);
		}
	}
}
