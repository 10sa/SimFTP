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

namespace SimFTP.Net
{
	public static class ShareNetUtil
	{
		public static InfoPacket ReceiveInfoPacket(Socket clientSocket)
		{
			PacketType packetType = GetPacketType(clientSocket);
			int dataCount = BitConverter.ToInt32(ShareNetUtil.ReceivePacket(clientSocket, sizeof(int)), 0);

			InfoType infoType = (InfoType)Enum.Parse(typeof(InfoType), ShareNetUtil.ReceivePacket(clientSocket, sizeof(byte))[0].ToString());
			int responseLenght = BitConverter.ToInt32(ShareNetUtil.ReceivePacket(clientSocket, sizeof(int)), 0);
			byte[] responseData = null;

			if(responseLenght > 0)
				responseData = ShareNetUtil.ReceivePacket(clientSocket, responseLenght);

			return new InfoPacket(infoType, responseData);
		}

		public static ErrorPacket ReceiveErrorPacket(Socket clientSocket, InfoPacket basePacket)
		{
			return new ErrorPacket((ErrorType)Enum.Parse(typeof(ErrorType), ReceivePacket(clientSocket, sizeof(byte))[0].ToString()));
		}

		public static byte[] ReceivePacket (Socket socket, int size)
		{
			byte[] buffer = new byte[size];
			socket.ReceiveTimeout = -1;

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

		public static int BufferedReceivePacket(Socket socket, byte[] buffer, int size)
		{
			try
			{
				return socket.Receive(buffer, size, SocketFlags.None);
			}
			catch(Exception)
			{
				socket.Close();
				throw;
			}
		}

		public static void SendInfoPacket(Socket clientSocket, InfoType type, byte[] data = null)
		{
			clientSocket.Send(new InfoPacket(type, data).GetBinaryData());
		}

		public static PacketType GetPacketType(Socket clientSocket)
		{
			byte[] data = ShareNetUtil.ReceivePacket(clientSocket, sizeof(byte));
			return (PacketType)Enum.Parse(typeof(PacketType), data[0].ToString());
		}

	}
}
