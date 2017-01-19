using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Collections;

using System.Net;
using System.Net.Sockets;


namespace Simple_File_Transfer.Net
{
	public delegate void ServerTransferCallback(string address, string statusMessage);
	public delegate void ClientTransferCallback();

	public class ServerTransfer : IDisposable
	{
		public const int ServerPort = 44335;

		#region Private Const field
		private const int MaximunBacklog = 16;
		#endregion

		#region Private field
		private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private List<Thread> threadList = new List<Thread>();
		private Thread serverThread;
		#endregion

		#region Public Callback field
		public event ServerTransferCallback ConnectingCallback = delegate { };
		public event ServerTransferCallback EndConnectingHandlingCallback = delegate { };
		public event ServerTransferCallback TransferStartCallback = delegate { };
		public event ServerTransferCallback TransferEndCallback = delegate { };

		public event ServerTransferCallback WrongCertificate = delegate { };
		public event ServerTransferCallback LowSecurityLevelPacket = delegate { };
		#endregion

		public ServerTransfer()
		{
			serverSocket.Bind(new IPEndPoint(IPAddress.Any, ServerPort));
			serverSocket.Listen(MaximunBacklog);
			serverSocket.NoDelay = false;

			serverThread  = new Thread(new ThreadStart(ConnectionHandleRoutine));
		}

		public void Start()
		{
			serverThread.Start();
		}

		public void Dispose()
		{
			serverSocket.Dispose();
			serverThread.Interrupt();

			foreach (var thread in threadList)
			{
				// blocking?
				thread.Interrupt();
				threadList.Remove(thread);
			}

			return;
		}

		#region Server Thread Area
		private void ConnectionHandleRoutine()
		{
			while(true)
			{
				Socket clientSocket = serverSocket.Accept();

				Thread connectHandleRoutine = new Thread(new ParameterizedThreadStart(ConnectHandleRoutine));
				connectHandleRoutine.Start(clientSocket);
			}
		}
		#endregion

		#region Handling Thread Area
		private void ConnectHandleRoutine(object threadArgs)
		{
			Socket clientSocket = (Socket)threadArgs;
			ClientConnectHandling(clientSocket);

			// File Transfer Handling //

			TransferStartCallback(GetClientAddress(clientSocket), "File Transfer Starting...");

			TransferEndCallback(GetClientAddress(clientSocket), "File Transfer End.");
		}

		private void ClientConnectHandling(Socket clientSocket)
		{
			clientSocket.ReceiveTimeout = 300;

			// Connect Handling //
			ConnectingCallback(GetClientAddress(clientSocket), "Client Connected, Connection Handling...");

			PacketType clientPacketType = GetPacketType(clientSocket);
			if(clientPacketType == PacketType.BasicFrame)
			{
				if(Util.GetConfigData("Accept_Default_Packet") == bool.TrueString)
				{
					short fileNameLenght = BitConverter.ToInt16(ReceivePacket(clientSocket, sizeof(short)), 0);
					long fileSize = BitConverter.ToInt64(ReceivePacket(clientSocket, sizeof(long)), 0);
					string fileName = Encoding.UTF8.GetString(ReceivePacket(clientSocket, fileNameLenght));

					byte[] fileData;
					if (fileSize > int.MaxValue)
					{
						fileData = ReceivePacket(clientSocket, int.MaxValue);
						for (long i = (fileSize - int.MaxValue); fileSize > int.MaxValue; fileSize -= int.MaxValue)
							fileData = Util.AttachByteArray(fileData, ReceivePacket(clientSocket, int.MaxValue));

						fileData = Util.AttachByteArray(fileData, ReceivePacket(clientSocket, (int)fileSize));
					}
					else
						fileData = ReceivePacket(clientSocket, (int)fileSize);

					Util.WriteFile(fileData, fileName);
					return;
				}
				else
				{
					// Send Error Type Packet.
					Console.WriteLine("PACKET NOT ACCPETED");
				}
				
			}

			EndConnectingHandlingCallback(GetClientAddress(clientSocket), "End Connecting Handling.");
		}

		

		private PacketType GetPacketType(Socket clientSocket)
		{
			byte[] data = ReceivePacket(clientSocket, sizeof(PacketType));
			return (PacketType)Enum.Parse(typeof(PacketType), BitConverter.ToInt32(data, 0).ToString());
		}
		#endregion

		#region Server Util
		private byte[] ReceivePacket(Socket socket, int size)
		{
			byte[] buffer = new byte[size];
			socket.ReceiveTimeout = 300;

			try
			{
				socket.Receive(buffer, size, SocketFlags.None);
				return buffer;
			}
			catch (Exception)
			{
				socket.Close();
				throw;
			}
		}

		private string GetClientAddress(Socket clientSocket)
		{
			return clientSocket.RemoteEndPoint.ToString();
		}
		#endregion
	}


	public class ClientTransfer : IDisposable
 	{
 		private const int ServerPort = 44332;
 		private Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
 
 		public event ClientTransferCallback ConnectingCallback;
 		public event ClientTransferCallback ConnectedCallback;
 
 		public ClientTransfer(string address)
 		{
 			try
 			{
 				ConnectingCallback();
 				clientSocket.Connect(address, ServerPort);
 				ConnectedCallback();
 
 				clientSocket.NoDelay = false;
 			}
 			catch(SocketException) { throw; }
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