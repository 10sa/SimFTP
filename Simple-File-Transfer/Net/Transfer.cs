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
		public event ServerTransferCallback TransferStartCallback;
		public event ServerTransferCallback TransferEndCallback;
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

		private void ConnectionHandleRoutine()
		{
			Socket clientSocket = serverSocket.Accept();
			ConnectingCallback(GetClientAddress(clientSocket), "Client Connected.");

			Thread connectHandleRoutine = new Thread(new ParameterizedThreadStart(ConnectHandleRoutine));
			connectHandleRoutine.Start(clientSocket);

			return;
		}

		private void ConnectHandleRoutine(object threadArgs)
		{
			Socket clientSocket = (Socket)threadArgs;

			TransferStartCallback(GetClientAddress(clientSocket), "File Transfer Starting...");



			TransferEndCallback(GetClientAddress(clientSocket), "File Transfer End.");

			return;
		}

		private string GetClientAddress(Socket clientSocket)
		{
			return clientSocket.RemoteEndPoint.ToString();
		}
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