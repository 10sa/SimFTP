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
		#region Private Const field
		private const int ServerPort = 44332;
		private const int MaximunBacklog = 16;
		#endregion

		#region Private field
		private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private List<Thread> threadList = new List<Thread>();
		private Thread serverThread;
		#endregion

		#region Public Callback field
		public event ServerTransferCallback ConnectingCallback;
		public event ServerTransferCallback EndConnectingHandlingCallback;
		public event ServerTransferCallback TransferStartCallback;
		public event ServerTransferCallback TransferEndCallback;

		public event ServerTransferCallback WrongCertificate;
		public event ServerTransferCallback LowSecurityLevelPacket;
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
			Socket clientSocket = serverSocket.Accept();
			
			Thread connectHandleRoutine = new Thread(new ParameterizedThreadStart(ConnectHandleRoutine));
			connectHandleRoutine.Start(clientSocket);

			return;
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

			return;
		}

		private void ClientConnectHandling(Socket clientSocket)
		{
			clientSocket.ReceiveTimeout = 300;

			// Connect Handling //
			ConnectingCallback(GetClientAddress(clientSocket), "Client Connected, Connection Handling...");

			PacketType clientPacketType = GetPacketType(clientSocket);
			if(clientPacketType == PacketType.BasicFrame)
			{
				if(Util.GetConfigData("Accepted_Default_Packet") == bool.TrueString)
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
				}
				else
				{

				}
				
			}

			EndConnectingHandlingCallback(GetClientAddress(clientSocket), "End Connecting Handling.");
		}

		

		private PacketType GetPacketType(Socket clientSocket)
		{
			return (PacketType)Enum.Parse(typeof(PacketType), ReceivePacket(clientSocket, sizeof(PacketType)).ToString());
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
 
 		public void SendingFile(byte[] file)
 		{
 			clientSocket.SendBufferSize = (int)file.LongLength / 4;
 			
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