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

	public class ServerTransfer
	{
		#region Private Const field
		private const int ServerPort = 44332;
		private const int MaximunBacklog = 16;
		#endregion

		#region Private field
		private Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		#endregion

		#region Public Callback field
		public event ServerTransferCallback ConnectingCallback;
		public event ServerTransferCallback TransferStartCallback;
		public event ServerTransferCallback TransferEndCallback;
		#endregion

		public ServerTransfer()
		{
			ServerSocket.Bind(new IPEndPoint(IPAddress.Any, ServerPort));
			ServerSocket.Listen(MaximunBacklog);
			
			Thread serverThread = new Thread(new ThreadStart(ConnectionHandleRoutine));
		}

		private void ConnectionHandleRoutine()
		{
			Socket clientSocket = ServerSocket.Accept();
			ConnectingCallback(clientSocket.RemoteEndPoint.ToString(), "Client Connected.");

			Thread clinetHandleThread = new Thread(new ParameterizedThreadStart(ClientHandleRoutine));
		}

		private void ClientHandleRoutine(object threadArgs)
		{
			Socket clientSocket = (Socket)threadArgs;
		}
	}
}