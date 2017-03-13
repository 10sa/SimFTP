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
	public class ServerUtil
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
			clientSocket.Shutdown(SocketShutdown.Send);
		}

		public static void SendInfoPacket(Socket clientSocket, InfoType type, byte[] data=null)
		{
			clientSocket.Send(new InfoPacket(type, data).GetBinaryData());
		}

		public static InfoPacket ReceiveInfoPacket(Socket clientSocket)
		{
			PacketType packetType = GetPacketType(clientSocket);
			int dataCount = BitConverter.ToInt32(ReceivePacket(clientSocket, sizeof(int)), 0);

			InfoType infoType = (InfoType)Enum.Parse(typeof(InfoType), ReceivePacket(clientSocket, sizeof(byte))[0].ToString());
			int responseLenght = BitConverter.ToInt32(ReceivePacket(clientSocket, sizeof(int)), 0);
			byte[] responseData = null;

			if(responseLenght >= 0)
				responseData = ReceivePacket(clientSocket, responseLenght);

			return new InfoPacket(infoType, responseData);
		}

		public static ErrorPacket ReceiveErrorPacket(Socket clientSocket, InfoPacket basePacket)
		{
			if(basePacket.Info != InfoType.Error)
				throw new ArgumentException("Wrong Base Packet.");

			return new ErrorPacket((ErrorType)Enum.Parse(typeof(ErrorType), ReceivePacket(clientSocket, sizeof(byte))[0].ToString()));
		}
	}
}
