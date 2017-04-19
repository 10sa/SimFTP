using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimFTP.Config
{
	public class TransferConfig : ConfigManager
	{
		public TransferConfig() : base("TransferConfig.cfg") { }

		protected override void InitializeConfig()
		{
			AddConfigTable("Accept_Default_Packet", bool.TrueString);

			AddConfigTable("Accept_Basic_Security_Packet", bool.TrueString);
			AddConfigTable("Accept_Anonymous_Login", bool.TrueString);

			AddConfigTable("Accpet_Expert_Security_Packet", bool.TrueString);
			AddConfigTable("Is_Overwrite", bool.FalseString);
			AddConfigTable("Is_Save_Date_Folder", bool.TrueString);
			AddConfigTable("Is_Save_User_Folder", bool.FalseString);
			AddConfigTable("Download_Folder", "");
		}
	}
}
