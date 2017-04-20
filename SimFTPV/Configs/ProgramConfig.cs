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
		public const string UsingProgramTray = "Using_Program_Tray";
		public const string NotifyAcceptEvent = "Notify_Packet_Accept";

		public ProgramConfig() : base("ProgramConfig.cfg") { }

		protected override void InitializeConfig()
		{
			ConfigTable.Add(UsingProgramTray, bool.TrueString);
			ConfigTable.Add(NotifyAcceptEvent, bool.FalseString);
		}
	}
}
