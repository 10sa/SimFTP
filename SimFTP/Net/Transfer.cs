using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Collections;

using SimFTP.Config;
using SimFTP.Net.MetadataPackets;
using SimFTP.Net.DataPackets;
using System.Net;
using System.Net.Sockets;


namespace SimFTP.Net
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

		private TransferConfig config = new TransferConfig();
		private AccountConfig accountConfig = new AccountConfig();

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
			BasicMetadataPacket packetData = ReceiveBasicMetadataPacket(clientSocket);

			switch (packetData.PacketType)
			{
				case PacketType.BasicFrame:
					BasicMetatdataPacketHandling(clientSocket, packetData);
					break;
				case PacketType.BasicSecurity:
					BasicSecurityMetadataPacketHandling(clientSocket, packetData);
					break;
				case PacketType.ExpertSecurity:
					break;
				case PacketType.Error:
					break;
			};
		}

		private void BasicMetatdataPacketHandling(Socket clientSocket, BasicMetadataPacket packetData)
		{
			if (config.GetConfigTable("Accept_Default_Packet") == bool.TrueString)
			{
				for (int i = 0; i < packetData.DataCount; i++)
				{
					BasicDataPacket data = ReceiveBasicDataPacket(clientSocket);
					Util.WriteFile(data.FileData, data.FileName);
				}
			}
			else
				SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);
		}

		private void BasicSecurityMetadataPacketHandling(Socket clientSocket, BasicMetadataPacket packetData)
		{
			if (config.GetConfigTable("Accept_Basic_Security_Packet") == bool.TrueString)
			{
				BasicSecurityMetadataPacket childPacket = ReceiveBasicSecurityMetadataPacket(clientSocket, packetData);

				if (childPacket.IsAnonynomus == true)
				{
					if (config.GetConfigTable("Accept_Anonymous_Login") == bool.TrueString)
					{
						BasicSecurityDataPacket data = ReceiveBasicSecurityDataPacket(clientSocket);
						Util.WriteFile(data.FileData, data.FileName);
					}
					else
						SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Anonymous);
				}
				else if (accountConfig.GetConfigTable(childPacket.Username) == Util.GetHashedString(childPacket.Password))
				{
					BasicSecurityDataPacket data = ReceiveBasicSecurityDataPacket(clientSocket);
					Util.WriteFile(data.FileData, data.FileName);
				}
				else
					SendErrorPacket(clientSocket, ErrorType.Wrong_Certificate);
			}
			else
				SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);
		}


		#endregion

		#region Metadata Packets Receive
		private static BasicMetadataPacket ReceiveBasicMetadataPacket(Socket clientSocket)
		{
			PacketType packetType = GetPacketType(clientSocket);
			int dataCount = BitConverter.ToInt32(ReceivePacket(clientSocket, sizeof(int)), 0);

			return new BasicMetadataPacket(dataCount);
		}

		private static BasicSecurityMetadataPacket ReceiveBasicSecurityMetadataPacket(Socket clientSocket, BasicMetadataPacket orignalPacket)
		{
			bool isAnonymous = BitConverter.ToBoolean(ReceivePacket(clientSocket, sizeof(bool)), 0);

			if (isAnonymous == true)
				return new BasicSecurityMetadataPacket(orignalPacket.DataCount);
			else
			{
				short usernameLenght = BitConverter.ToInt16(ReceivePacket(clientSocket, sizeof(short)), 0);
				short passwordLenght = BitConverter.ToInt16(ReceivePacket(clientSocket, sizeof(short)), 0);
				string username = Encoding.UTF8.GetString(ReceivePacket(clientSocket, usernameLenght));
				string password = Encoding.UTF8.GetString(ReceivePacket(clientSocket, passwordLenght));

				return new BasicSecurityMetadataPacket(username, password, orignalPacket.DataCount);
			}
		}
		#endregion

		#region Data Packets Receive
		private static BasicDataPacket ReceiveBasicDataPacket(Socket clientSocket)
		{
			short fileNameLenght = BitConverter.ToInt16(ReceivePacket(clientSocket, sizeof(short)), 0);
			long fileSize = BitConverter.ToInt64(ReceivePacket(clientSocket, sizeof(long)), 0);
			string fileName = Encoding.UTF8.GetString(ReceivePacket(clientSocket, fileNameLenght));
			byte[] fileData = GetFileData(clientSocket, fileSize);

			return new BasicDataPacket(fileNameLenght, fileSize, fileName, fileData);
		}

		private static BasicSecurityDataPacket ReceiveBasicSecurityDataPacket(Socket clientSocket)
		{
			BasicDataPacket data = ReceiveBasicDataPacket(clientSocket);
			return new BasicSecurityDataPacket(data.FileName, data.FileNameLenght, data.FileData, data.FileSize, ReceivePacket(clientSocket, Util.HashByteSize));
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
			byte[] data = ReceivePacket(clientSocket, sizeof(byte));
			return (PacketType)Enum.Parse(typeof(PacketType), data[0].ToString());
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