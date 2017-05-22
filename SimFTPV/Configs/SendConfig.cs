using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimFTP.Config;
using SimFTP.Net.MetadataPackets;
using SimFTP.Enums;

namespace SimFTPV.Configs
{
	public class SendConfig : ConfigManager
	{
		public SendConfig() : base("SendConfigs.cfg") { }

		protected override void InitializeConfig()
		{
			AddConfigTable("Using_Mode", PacketType.BasicFrame.ToString());
			AddConfigTable("Auth_Username", "");
			AddConfigTable("Auth_Password", "");
			AddConfigTable("Auth_Anonymous", bool.TrueString);
		}
	}
}
