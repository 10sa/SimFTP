using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimFTP.Config
{
	public class TransferConfig : ConfigManager
	{
		public const string AcceptDefaultPacket = "Accept_Default_Packet";
		public const string AcceptBasicSecurityPacket = "Accept_Basic_Security_Packet";
		public const string AccpetAnonymousUser = "Accept_Anonymous_Login";
		public const string AcceptExpertSecurityPacket = "Accpet_Expert_Security_Packet";
		public const string IsOverwrite = "Is_Overwrite";
		public const string IsSaveDateDirectory = "Is_Save_Date_Directory";
		public const string IsSaveUserDirectory = "Is_Save_User_Directory";
		public const string DownloadDirectory = "Download_Directory";

		public TransferConfig() : base("TransferConfig.cfg") { }

		protected override void InitializeConfig()
		{
			AddConfigTable(AcceptDefaultPacket, bool.TrueString);

			AddConfigTable(AcceptBasicSecurityPacket, bool.TrueString);
			AddConfigTable(AccpetAnonymousUser, bool.TrueString);

			AddConfigTable(AcceptExpertSecurityPacket, bool.TrueString);
			AddConfigTable(IsOverwrite, bool.FalseString);
			AddConfigTable(IsSaveDateDirectory, bool.TrueString);
			AddConfigTable(IsSaveUserDirectory, bool.FalseString);
			AddConfigTable(DownloadDirectory, "");
		}
	}
}
