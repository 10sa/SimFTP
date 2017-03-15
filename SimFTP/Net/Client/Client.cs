﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;


using SimFTP.Config;
using SimFTP.Net.MetadataPackets;
using SimFTP.Net.DataPackets;
using SimFTP.Security;

using System.Net;
using System.Net.Sockets;


namespace SimFTP.Net.Client
{
	public class Client : IDisposable
	{
		private readonly string ServerAddress;
		private const int ServerPort = 44335;
		private Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private PacketType SendType;

		public Client(string address, PacketType sendingType)
		{
			clientSocket.NoDelay = false;
			ServerAddress = address;

			SendType = sendingType;
		}

		public void SendFile(params BasicDataPacket[] files)
		{
			if(SendType == PacketType.BasicFrame)
			{
				clientSocket.Connect(ServerAddress, ServerPort);
				SendHandlingBasicPackets(files);
			}
			else
				ThrowFormattedException(PacketType.BasicFrame);
		}

		public void SendFile(string username, string password, params BasicSecurityDataPacket[] files)
		{
			if(SendType == PacketType.BasicSecurity)
			{
				clientSocket.Connect(ServerAddress, ServerPort);
				SendHandlingBasicSecurityPacket(username, password, files);
			}
			else
				ThrowFormattedException(PacketType.BasicSecurity);
		}

		public void SendFile(params BasicSecurityDataPacket[] files)
		{
			if(SendType == PacketType.BasicSecurity)
			{
				clientSocket.Connect(ServerAddress, ServerPort);
				SendHandlingBasicSecurityPacket(files);
			}
			else
				ThrowFormattedException(PacketType.BasicSecurity);
		}

		public void SendFile(params ExpertSecurityDataPacket[] files)
		{
			if(SendType == PacketType.ExpertSecurity)
			{
				clientSocket.Connect(ServerAddress, ServerPort);
			}
		}

		private void ThrowFormattedException(PacketType tryType)
		{
			throw new InvalidOperationException(string.Format("Wrong Packet Type. Tyring sending {0} type packet But, class sending type is {1}.", tryType, SendType));
		}

		private void SendHandlingBasicPackets(params BasicDataPacket[] files)
		{
			clientSocket.Send(new BasicMetadataPacket(files.Length).GetBinaryData());

			InfoPacket infoPacket = InfoExchangeHandler();

			// TO DO : CALL INFO PACKET RECEIVE EVENT. (Arg : InfoPacket) //

			//
			FileSendHandler(files);

			clientSocket.Shutdown(SocketShutdown.Send);
		}

		// Anonymous Send //
		private void SendHandlingBasicSecurityPacket(params BasicSecurityDataPacket[] files)
		{
			clientSocket.Send(new BasicSecurityMetadataPacket(files.Length).GetBinaryData());

			InfoPacket infoPacket = InfoExchangeHandler();
			// TO DO : CALL INFO PACKET RECEIVE EVENT. (Arg : InfoPacket) //

			//

			FileSendHandler(files);
			clientSocket.Shutdown(SocketShutdown.Send);
		}

		// User Auth Send //
		private void SendHandlingBasicSecurityPacket(string username, string password, params BasicSecurityDataPacket[] files)
		{
			clientSocket.Send(new BasicSecurityMetadataPacket(username, password, files.Length).GetBinaryData());
			InfoPacket infoPacket = InfoExchangeHandler();

			// TO DO : CALL INFO PACKET RECEIVE EVENT. (Arg : InfoPacket) //

			//

			FileSendHandler(files);
			clientSocket.Shutdown(SocketShutdown.Send);
		}

		// Anonymous Encrypted Send //
		private void SendHandlingExpertSecurityPacket(params ExpertSecurityDataPacket[] files)
		{
			HandlingExpertSecurityExchange(files);
		}

		private void SendHandlingExpertSecurityPacket(string username, string password, params ExpertSecurityDataPacket[] files)
		{
			HandlingExpertSecurityExchange(files, username, password);
		}

		private void HandlingExpertSecurityExchange(ExpertSecurityDataPacket[] files, string username = null, string password = null)
		{
			using(DH521Manager dhManager = new DH521Manager())
			{
				if(username != null && password != null)
					clientSocket.Send(new ExpertSecurityMetadataPacket(files.Length, username, password, dhManager.PublicKey).GetBinaryData());
				else
					clientSocket.Send(new ExpertSecurityMetadataPacket(files.Length, dhManager.PublicKey).GetBinaryData());

				InfoPacket infoPacket = InfoExchangeHandler();
				ShareNetUtil.SendInfoPacket(clientSocket, InfoType.ShareKey, dhManager.PublicKey);
				byte[] shareKey = dhManager.GetShareKey(infoPacket.ResponseData);

				using(AES256Manager aesManager = new AES256Manager(shareKey))
				{
					foreach(var file in files)
					{
						file.SetFileData(aesManager.Encrypt(file.FileData));
						clientSocket.Send(file.GetBinaryData());
					}

					clientSocket.Shutdown(SocketShutdown.Send);
				}
			}
		}

		private void FileSendHandler(params BasicDataPacket[] files)
		{
			foreach(var file in files)
				clientSocket.Send(file.GetBinaryData());
		}

		private InfoPacket InfoExchangeHandler()
		{
			InfoPacket receivedInfo = ShareNetUtil.ReceiveInfoPacket(clientSocket);

			if (receivedInfo.Info == InfoType.Accept || receivedInfo.Info == InfoType.ShareKey)
				return receivedInfo;
			else if(receivedInfo.Info == InfoType.Error)
			{
				ErrorPacket error = ShareNetUtil.ReceiveErrorPacket(clientSocket);
				clientSocket.Close();

				throw new SecurityException(string.Format("Received Error Packet. ERROR CODE : {0}", error.ErrorType.ToString()));
			}
			else
			{
				clientSocket.Close();

				throw new SecurityException(string.Format("Received Othres Status Packet. PACKET CODE {0}", receivedInfo.PacketType.ToString()));
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
