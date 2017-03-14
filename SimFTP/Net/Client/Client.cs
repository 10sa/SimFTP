using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;


using SimFTP.Config;
using SimFTP.Net.MetadataPackets;
using SimFTP.Net.DataPackets;
using System.Net;
using System.Net.Sockets;


namespace SimFTP.Net.Client
{
	public class Client : IDisposable
	{
		private readonly string ServerAddress;
		private const int ServerPort = 44335;
		private Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private PacketType SendType;

		public Client(string address, PacketType sendingType)
		{
			clientSocket.NoDelay = false;
			ServerAddress = address;

			SendType = sendingType;
		}

		public void SendingFile(params BasicDataPacket[] files)
		{
			if(SendType == PacketType.BasicFrame)
			{
				clientSocket.Connect(ServerAddress, ServerPort);
				SendHandlingBasicPackets(files);
			}
			else
				throw new InvalidOperationException(string.Format("Wrong Packet Type. SEND TYPE {0}", SendType));
		}

		public void SendFile(params BasicSecurityDataPacket[] files)
		{
			SendHandlingBasicSecurityPacket(files);
		}

		private void SendHandlingBasicPackets(params BasicDataPacket[] files)
		{
			clientSocket.Send(new BasicMetadataPacket(files.Length).GetBinaryData());

			InfoPacket infoPacket = InfoExchangeHandler();

			// TO DO : CALL INFOPACKET RECEIVE EVENT. (Arg : InfoPacket) //
			foreach(var file in files)
				clientSocket.Send(file.GetBinaryData());

			clientSocket.Shutdown(SocketShutdown.Send);
		}

		private void SendHandlingBasicSecurityPacket(params BasicSecurityDataPacket[] files)
		{
			
		}

		private InfoPacket InfoExchangeHandler()
		{
			InfoPacket receivedInfo = ShareNetUtil.ReceiveInfoPacket(clientSocket);

			if (receivedInfo.Info == InfoType.Accept)
				return receivedInfo;
			else if(receivedInfo.Info == InfoType.Error)
			{
				ErrorPacket error = ShareNetUtil.ReceiveErrorPacket(clientSocket);
				clientSocket.Close();

				throw new SecurityException(string.Format("Received Error Packet. ERROR CODE : {0}", error.ErrorType.ToString()));
			}
			else
			{
				clientSocket.Close();

				throw new SecurityException(string.Format("Received Othres Status Packet. PACKET CODE {0}", receivedInfo.PacketType.ToString()));
			}
		}

		public void Dispose()
		{
			if(clientSocket.Connected)
				clientSocket.Disconnect(false);

			clientSocket.Dispose();
			return;
		}
	}
}
