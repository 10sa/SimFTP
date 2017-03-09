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
	class ServerUtil
	{
		public static string GetClientAddress (Socket clientSocket)
		{
			return clientSocket.RemoteEndPoint.ToString();
		}

		public static byte[] ReceivePacket (Socket socket, int size)
		{
			byte[] buffer = new byte[size];
			socket.ReceiveTimeout = 300;

			try
			{
				socket.Receive(buffer, size, SocketFlags.None);
				return buffer;
			}
			catch(Exception)
			{
				socket.Close();
				throw;
			}
		}

		public static PacketType GetPacketType (Socket clientSocket)
		{
			byte[] data = ReceivePacket(clientSocket, sizeof(byte));
			return (PacketType)Enum.Parse(typeof(PacketType), data[0].ToString());
		}

		public static void SendErrorPacket (Socket clientSocket, ErrorType error)
		{
			clientSocket.Send(new ErrorPacket(error).GetBinaryData());
			clientSocket.Close();
		}
	}
}
