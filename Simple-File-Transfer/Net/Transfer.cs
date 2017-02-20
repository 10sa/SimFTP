using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using Simple_File_Transfer.Util;

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
		public event ServerTransferCallback ServerTransferCallbackEvent = delegate { };
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
		}

		private void ClientConnectHandling(Socket clientSocket)
		{
			clientSocket.ReceiveTimeout = 300;
			PacketFrame packetData = GetPacketData(clientSocket);

			switch (packetData.PacketType)
			{
				case PacketType.BasicFrame:
					if (ConfigUtil.GetConfigData("Accept_Default_Packet") == bool.TrueString)
					{
						for(int i=0; i < packetData.DataCount; i++)
						{
							Console.WriteLine("File Received." + i.ToString());
							DataFrame data = ReceiveDataFramePacket(clientSocket);
							Utils.WriteFile(data.FileData, data.FileName);
						}
					}
					else
						SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);

					break;
				case PacketType.BasicSecurity:
					if (ConfigUtil.GetConfigData("Accept_Basic_Security_Packet") == bool.TrueString)
					{
						BasicSecurityPacket childPacket = GetBasicSecurityPacketData(clientSocket, packetData);

						if (childPacket.IsAnonynomus == true)
						{
							if (ConfigUtil.GetConfigData("Accept_Anonymous_Login") == bool.TrueString)
							{

							}
							else
								SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Anonymous);
						}
						else
						{
							if(ConfigUtil.GetAccountPassword(childPacket.Username) == Utils.GetHashedString(childPacket.Password))
							{

							}
						}
					}
					else
						SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);
					break;
				case PacketType.ExpertsSecurity:
					break;
				case PacketType.Error:
					break;
			};
		}

		
		#endregion

		#region Packet Frame Data Receive Code
		private static PacketFrame GetPacketData(Socket clientSocket)
		{
			PacketType packetType = GetPacketType(clientSocket);
			byte dataCount = ReceivePacket(clientSocket, sizeof(byte))[0];

			return new PacketFrame(dataCount);
		}

		private static BasicSecurityPacket GetBasicSecurityPacketData(Socket clientSocket, PacketFrame orignalPacket)
		{
			bool isAnonymous = BitConverter.ToBoolean(ReceivePacket(clientSocket, sizeof(bool)), 0);

			if (isAnonymous == true)
				return new BasicSecurityPacket(orignalPacket.DataCount);
			else
			{
				short usernameLenght = BitConverter.ToInt16(ReceivePacket(clientSocket, sizeof(short)), 0);
				short passwordLenght = BitConverter.ToInt16(ReceivePacket(clientSocket, sizeof(short)), 0);
				string username = Encoding.UTF8.GetString(ReceivePacket(clientSocket, usernameLenght));
				string password = Encoding.UTF8.GetString(ReceivePacket(clientSocket, passwordLenght));

				return new BasicSecurityPacket(username, password, orignalPacket.DataCount);
			}
		}
		#endregion

		#region Data Frame Packet Receive Code
		private static DataFrame ReceiveDataFramePacket(Socket clientSocket)
		{
			short fileNameLenght = BitConverter.ToInt16(ReceivePacket(clientSocket, sizeof(short)), 0);
			long fileSize = BitConverter.ToInt64(ReceivePacket(clientSocket, sizeof(long)), 0);
			string fileName = Encoding.UTF8.GetString(ReceivePacket(clientSocket, fileNameLenght));
			byte[] fileData = GetFileData(clientSocket, fileSize);

			return new DataFrame(fileNameLenght, fileSize, fileName, fileData);
		}

		private static byte[] GetFileData(Socket clientSocket, long fileSize)
		{
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

			return fileData;
		}
		#endregion

		#region Server Util
		private static byte[] ReceivePacket(Socket socket, int size)
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

		private static string GetClientAddress(Socket clientSocket)
		{
			return clientSocket.RemoteEndPoint.ToString();
		}

		private static PacketType GetPacketType(Socket clientSocket)
		{
			byte[] data = ReceivePacket(clientSocket, sizeof(PacketType));
			return (PacketType)Enum.Parse(typeof(PacketType), BitConverter.ToInt32(data, 0).ToString());
		}

		private static void SendErrorPacket(Socket clientSocket, ErrorType error)
		{
			clientSocket.Send(new ErrorPacket(error).GetBinaryData());
			clientSocket.Close();
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