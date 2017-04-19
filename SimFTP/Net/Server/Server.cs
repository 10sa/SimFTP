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
	public delegate void ServerTransferEvent (ServerEventArgs args);

	public class Server : IDisposable
	{
		public const int ServerPort = 44335;

		#region Private Const field
		private const int MaximunBacklog = 16;
		#endregion

		#region Private field
		public Socket serverSocket { get; private set; } = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private List<Thread> threadList = new List<Thread>();
		private Thread serverThread;

		public TransferConfig config = new TransferConfig();
		public AccountConfig accountConfig = new AccountConfig();

		private ManualResetEvent threadEvent = new ManualResetEvent(false);
		private ManualResetEvent threadControllEvent = new ManualResetEvent(false);
		private ManualResetEvent lastSubroutineEvent;

		#endregion

		#region Public Callback field

		public event ServerTransferEvent ConnectedClient = delegate { };

		public event ServerTransferEvent AcceptBasicPacket = delegate { };
		public event ServerTransferEvent ClientSendErrorPacekt = delegate { };
		public event ServerTransferEvent ReceiveErrorData = delegate { };

		public event ServerTransferEvent ReceivedBasicPacket = delegate { };
		public event ServerTransferEvent ReceivedBasicSecurityPacket = delegate { };
		public event ServerTransferEvent ReceivedExpertSecurityPacket = delegate { };
		public event ServerTransferEvent ReceivedErrorPacket = delegate { };
		public event ServerTransferEvent ReceivedInvaildPacket = delegate { };
		public event ServerTransferEvent ReceiveEnd = delegate { };

		#endregion

		#region Public Var
		public bool IsRunning { get { return threadControllEvent.WaitOne(0); } }
		#endregion

		public Server()
		{
			serverSocket.Bind(new IPEndPoint(IPAddress.Any, ServerPort));
			serverSocket.Listen(MaximunBacklog);
			serverSocket.NoDelay = false;

			serverThread = new Thread(new ThreadStart(ConnectionHandleRoutine));
			serverThread.Name = "Server Connection Handler";
			serverThread.Start();
		}

		public void Start ()
		{
			threadControllEvent.Set();
		}

		public void Stop()
		{
			threadControllEvent.Reset();
		}

		public void Dispose ()
		{
			threadControllEvent.Dispose();
			threadEvent.Dispose();
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
			SocketAsyncEventArgs serverSocketArgs = new SocketAsyncEventArgs();
			serverSocketArgs.Completed += this.AccpetCallBack;

			threadEvent.Set();

			while(true)
			{
				try {
					// For Async Call. //
					threadControllEvent.WaitOne();
					threadEvent.WaitOne();

					serverSocket.AcceptAsync(serverSocketArgs);

					threadEvent.Reset();
				}
				catch(ThreadInterruptedException) { }
			}
		}

		private struct ThreadArgs
		{
			public Socket clientSocket;
			public ManualResetEvent threadControl;
			public ManualResetEvent outputControl;

			public ThreadArgs(Socket socket, ManualResetEvent controlEvent)
			{
				clientSocket = socket;
				threadControl = controlEvent;
				outputControl = null;
			}
		}

		private void AccpetCallBack(object sender, SocketAsyncEventArgs callbackArgs)
		{
			if(callbackArgs.AcceptSocket == null || !callbackArgs.AcceptSocket.Connected)
				return;

			Thread connectHandleRoutine = new Thread(new ParameterizedThreadStart(ConnectHandleRoutine));
			Socket clientSocket = callbackArgs.AcceptSocket;
			ThreadArgs threadArgs = new ThreadArgs(clientSocket, lastSubroutineEvent);

			connectHandleRoutine.Name = string.Format("{0}_Client_IO_Handler", clientSocket.Handle);
			ConnectedClient(new ServerEventArgs(null, clientSocket.RemoteEndPoint.ToString()));
			connectHandleRoutine.Start(threadArgs);

			callbackArgs.AcceptSocket = null;
			threadEvent.Set();
		}
		#endregion

		#region Handling Thread Area
		private void ConnectHandleRoutine (object threadArgs)
		{
			ThreadArgs args = (ThreadArgs)threadArgs;
			Socket clientSocket = args.clientSocket;
			ManualResetEvent waitEvent = args.threadControl;
			ManualResetEvent localEvent = new ManualResetEvent(false);
			lastSubroutineEvent = localEvent;

			if(waitEvent != null)
				waitEvent.WaitOne();

			try
			{
				ClientConnectHandling(clientSocket);
			}
			finally
			{
				localEvent.Set();
			}
		}

		private void ClientConnectHandling (Socket clientSocket)
		{
			clientSocket.ReceiveTimeout = 300;
			ServerPacketHandler handler = new ServerPacketHandler(clientSocket, ref config, ref accountConfig);

			handler.ReceivedBasicPacket += ReceivedBasicPacket;
			handler.ReceivedBasicSecurityPacket += ReceivedBasicSecurityPacket;
			handler.ReceivedExpertSecurityPacket += ReceivedExpertSecurityPacket;
			handler.ReceivedErrorPacket += ReceivedErrorPacket;
			handler.ReceivedInvaildPacket += ReceivedInvaildPacket;
			handler.ReceiveEnd += ReceiveEnd;

			handler.StartHandling();
		}
		#endregion
	}
}
