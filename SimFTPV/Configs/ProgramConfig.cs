using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimFTP.Config;

namespace SimFTPV.Configs
{
	public class ProgramConfig : ConfigManager
	{
		public ProgramConfig() : base("ProgramConfig.cfg") { }

		protected override void InitializeConfig()
		{
			ConfigTable.Add("Using_Program_Tray", bool.TrueString);
			ConfigTable.Add("Notify_Packet_Accept", bool.FalseString);
		}
	}
}
