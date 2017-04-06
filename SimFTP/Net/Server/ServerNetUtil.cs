using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimFTP;
using SimFTP.Config;

using SimFTP.Net;
using SimFTP.Net.DataPackets;
using SimFTP.Net.MetadataPackets;

using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace SimFTP.Net.Server
{
	public static class ServerNetUtil
	{
		public static void SendErrorPacket (Socket clientSocket, ErrorType error)
		{
			clientSocket.Send(new ErrorPacket(error).GetBinaryData());
			clientSocket.Close(30);
		}
	}
}
