using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using SimFTP.Config;
using SimFTP.Net.MetadataPackets;
using SimFTP.Net.DataPackets;
using System.Net;
using System.Net.Sockets;


namespace SimFTP.Net.Client
{
	class Client
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

			public void SendingFile(params byte[][] files)
			{
				clientSocket.Connect(ServerAddress, ServerPort);

				switch (SendType)
				{
					case PacketType.BasicFrame:
						break;
				}
			}

			private void SendHandlingBasicPackets(params byte[][] files)
			{
				clientSocket.Send(new BasicMetadataPacket(files.Length).GetBinaryData());


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
}
