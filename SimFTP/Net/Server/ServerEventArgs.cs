using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimFTP.Net.Server
{
	public class ServerEventArgs : EventArgs
	{
		public string StatusMessage { get; set; }

		public string ClientAddress { get; set; }

		public bool Cancel { get; set; }

		private ServerEventArgs() { }

		public ServerEventArgs(string status, string address)
		{
			StatusMessage = status;
			ClientAddress = address;
			Cancel = false;
		}
	}
}
