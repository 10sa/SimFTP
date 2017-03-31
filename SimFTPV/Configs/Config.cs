using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimFTP.Config;
using SimFTP.Net.MetadataPackets;

namespace SimFTPV
{
	public class ClientConfig : ConfigManager
	{
		public ClientConfig() : base("ProgramConfigs.cfg") { }

		protected override void InitializeConfig()
		{
			AddConfigTable("Using_Mode", PacketType.BasicFrame.ToString());
			AddConfigTable("Auth_Username", "");
			AddConfigTable("Auth_Password", "");
			AddConfigTable("Auth_Anonymous", bool.TrueString);
			AddConfigTable("Using_Tray_Mode", bool.TrueString);
		}
	}
}
