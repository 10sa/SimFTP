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
		public AccountConfig accountConfig = new AccountConfig();
		private ManualResetEvent threadEvent = new ManualResetEvent(false);

		#endregion

		#region Public Callback field
		public event ServerTransferCallback ServerTransferCallbackEvent = delegate { };
		public bool IsRunning { get { return threadEvent.WaitOne(0); } }
		#endregion

		public Server()
		{
			serverSocket.Bind(new IPEndPoint(IPAddress.Any, ServerPort));
			serverSocket.Listen(MaximunBacklog);
			serverSocket.NoDelay = false;

			serverThread = new Thread(new ThreadStart(ConnectionHandleRoutine));
			serverThread.Start();
		}

		public void Start ()
		{
			threadEvent.Set();
		}

		public void Stop()
		{
			threadEvent.Reset();
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
			try
			{
				threadEvent.WaitOne();
			}
			catch(ThreadInterruptedException) { }

			SocketAsyncEventArgs saea = new SocketAsyncEventArgs();
			saea.Completed += (a, b) =>
			{
				if(b.ConnectSocket != null)
				{
					b.ConnectSocket.NoDelay = false;

					Thread connectHandleRoutine = new Thread(new ParameterizedThreadStart(ConnectHandleRoutine));
					connectHandleRoutine.Name = string.Format("Client_IO_Handler", b.ConnectSocket.Handle);
					connectHandleRoutine.Start(b.ConnectSocket);
				}

				threadEvent.WaitOne();
				serverSocket.AcceptAsync(saea);
			};

			serverSocket.AcceptAsync(saea);

			while(true)
			{
				try {
					// For Async Call. //
					threadEvent.WaitOne();
					Thread.Sleep(10);
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
