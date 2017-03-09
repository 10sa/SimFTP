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
			private const int ServerPort = 44332;
			private Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			public event ClientTransferCallback ConnectingCallback;
			public event ClientTransferCallback ConnectedCallback;

			public ClientTransfer(string address)
			{
				ConnectingCallback();
				clientSocket.Connect(address, ServerPort);
				ConnectedCallback();

				clientSocket.NoDelay = false;
			}

			public void SendingFile(byte[] file, PacketType packetType)
			{
				clientSocket.SendBufferSize = (int)file.LongLength / 4;

				if(packetType == PacketType.BasicFrame)
				{

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
}
