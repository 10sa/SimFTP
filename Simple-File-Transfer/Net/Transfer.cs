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
						if (Util.GetConfigData("Accept_Default_Packet") == bool.TrueString)
						{
							DataFrame data = ReceiveDataFramePacket(clientSocket);
							Util.WriteFile(data.FileData, data.FileName);
						}
						else
						{
							clientSocket.Send(new ErrorPacket(ErrorType.Not_Accepted_Default_Packet).GetBinaryData());
							clientSocket.Close();
						}

						break;
				case PacketType.BasicSecurity:
					break;
				case PacketType.ExpertsSecurity:
					break;
				case PacketType.Error:
					break;
			};
		}
		#endregion

		#region Packet Frame Data Receive Code
		private PacketFrame GetPacketData(Socket clientSocket)
		{
			PacketType packetType = GetPacketType(clientSocket);
			byte dataCount = ReceivePacket(clientSocket, sizeof(byte))[0];

			return new PacketFrame(dataCount);
		}
		#endregion

		#region Data Frame Packet Receive Code
		private DataFrame ReceiveDataFramePacket(Socket clientSocket)
		{
			short fileNameLenght = BitConverter.ToInt16(ReceivePacket(clientSocket, sizeof(short)), 0);
			long fileSize = BitConverter.ToInt64(ReceivePacket(clientSocket, sizeof(long)), 0);
			string fileName = Encoding.UTF8.GetString(ReceivePacket(clientSocket, fileNameLenght));
			byte[] fileData = GetFileData(clientSocket, ref fileSize);

			return new DataFrame(fileNameLenght, fileSize, fileName, fileData);
		}

		private byte[] GetFileData(Socket clientSocket, ref long fileSize)
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

		private PacketType GetPacketType(Socket clientSocket)
		{
			byte[] data = ReceivePacket(clientSocket, sizeof(PacketType));
			return (PacketType)Enum.Parse(typeof(PacketType), BitConverter.ToInt32(data, 0).ToString());
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