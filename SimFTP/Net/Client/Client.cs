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
	public delegate void ClientTransferCallback();

	public class ClientTransfer : IDisposable
	{
		private readonly string ServerAddress;
		private const int ServerPort = 44332;
		private Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private PacketType SendType;

		public ClientTransfer(string address, PacketType sendingType)
		{
			clientSocket.NoDelay = false;
			ServerAddress = address;

			SendType = sendingType;
		}

		public void SendingFile(BasicDataPacket[] files)
		{
			clientSocket.Connect(ServerAddress, ServerPort);

			switch(SendType)
			{
				case PacketType.BasicFrame:
					SendHandlingBasicPackets(files);
					break;
			}
		}

		private void SendHandlingBasicPackets(params BasicDataPacket[] files)
		{
			clientSocket.Send(new BasicMetadataPacket(files.Length).GetBinaryData());

			InfoPacket infoPacket = ShareNetUtil.ReceiveInfoPacket(clientSocket);

			if(infoPacket.Info == InfoType.Accept)
			{
				foreach(var file in files)
					clientSocket.Send(file.GetBinaryData());

				clientSocket.Shutdown(SocketShutdown.Send);
			}
			else if(infoPacket.Info == InfoType.Error)
			{
				ErrorPacket error = ShareNetUtil.ReceiveErrorPacket(clientSocket, infoPacket);
				clientSocket.Close();

				throw new SecurityException(string.Format("Not Accepted Basic Packet. CODE : {0}", error.ErrorType.ToString()));
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
