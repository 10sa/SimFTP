using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimFTP;
using SimFTP.Config;

using SimFTP.Net;
using SimFTP.Net.DataPackets;
using SimFTP.Net.MetadataPackets;
using SimFTP.Net.Server.PacketHandlers;

using System.Net;
using System.Threading;
using System.Net.Sockets;


namespace SimFTP.Net.Server
{
	public delegate void ServerTransferCallback (string address, string statusMessage);

	public class Server : IDisposable
	{
		public const int ServerPort = 44335;

		#region Private Const field
		private const int MaximunBacklog = 16;
		#endregion

		#region Private field
		private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private List<Thread> threadList = new List<Thread>();
		private Thread serverThread;

		public TransferConfig config = new TransferConfig();
		private AccountConfig accountConfig = new AccountConfig();

		#endregion

		#region Public Callback field
		public event ServerTransferCallback ServerTransferCallbackEvent = delegate { };
		public bool IsRunning { get { if(serverThread.ThreadState == ThreadState.Running) { return true; } else { return false; } } }
		#endregion

		public Server()
		{
			serverSocket.Bind(new IPEndPoint(IPAddress.Any, ServerPort));
			serverSocket.Listen(MaximunBacklog);
			serverSocket.NoDelay = false;

			serverThread = new Thread(new ThreadStart(ConnectionHandleRoutine));
		}

		public void Start ()
		{
			serverThread.Start();
		}

		public void Stop()
		{
			// It's Really Not Looks good...
			serverThread.Interrupt();
		}

		public void AddPermission(string username, string password)
		{
			accountConfig.AddConfigTable(username, password);
			accountConfig.SaveData();
		}

		public void Dispose ()
		{
			serverSocket.Dispose();
			accountConfig.Dispose();
			config.Dispose();

			serverThread.Interrupt();

			foreach(var thread in threadList)
			{
				// blocking?
				thread.Interrupt();
				threadList.Remove(thread);
			}

			serverThread.Abort(null);
			return;
		}

		#region Server Thread Area
		private void ConnectionHandleRoutine ()
		{
			while(true)
			{
				SocketAsyncEventArgs saea = new SocketAsyncEventArgs();
				try
				{
					serverSocket.AcceptAsync(saea);
				}
				catch(ObjectDisposedException) { }
				

				if(saea.ConnectSocket != null)
				{
					saea.AcceptSocket.NoDelay = false;

					Thread connectHandleRoutine = new Thread(new ParameterizedThreadStart(ConnectHandleRoutine));
					connectHandleRoutine.Name = string.Format("Client_IO_Handler", saea.AcceptSocket.Handle);
					connectHandleRoutine.Start(saea.AcceptSocket);
				}


				try {
					Thread.Sleep(1);
				}
				catch(ThreadInterruptedException) { }
			}
		}
		#endregion

		#region Handling Thread Area
		private void ConnectHandleRoutine (object threadArgs)
		{
			Socket clientSocket = (Socket)threadArgs;
			ClientConnectHandling(clientSocket);
		}

		private void ClientConnectHandling (Socket clientSocket)
		{
			clientSocket.ReceiveTimeout = 300;
			ServerPacketHandler handler = new ServerPacketHandler(clientSocket, ref config, ref accountConfig);
		}
		#endregion
	}
}
