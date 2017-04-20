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
using SimFTP.Security;

using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace SimFTP.Net.Server.PacketHandlers
{
	internal class ServerPacketHandler
	{
		private const string AnonymousFolderName = "Anonymous";
		private readonly AccountConfig accountConfig;
		private readonly TransferConfig config;

		private MetadataPacketHandler metadataHandler;
		private DataPacketHandler dataHandler;
		private Socket clientSocket;

		#region Event Define

		public event ServerTransferEvent ReceivedBasicPacket = delegate { };
		public event ServerTransferEvent ReceivedBasicSecurityPacket = delegate { };
		public event ServerTransferEvent ReceivedExpertSecurityPacket = delegate { };
		public event ServerTransferEvent ReceivedErrorPacket = delegate { };
		public event ServerTransferEvent ReceivedInvaildPacket = delegate { };
		public event ServerTransferEvent ReceiveEnd = delegate { };

		#endregion

		public ServerPacketHandler(Socket clientSocket, ref TransferConfig config, ref AccountConfig accountConfig)
		{
			this.config = config;
			this.accountConfig = accountConfig;

			metadataHandler = new MetadataPacketHandler(clientSocket);
			dataHandler = new DataPacketHandler(clientSocket, 
				config.GetConfigTable(TransferConfig.DownloadDirectory), 
				bool.Parse(config.GetConfigTable(TransferConfig.IsSaveDateDirectory)), 
				bool.Parse(config.GetConfigTable(TransferConfig.IsOverwrite)),
				bool.Parse(config.GetConfigTable(TransferConfig.IsSaveUserDirectory))
			);

			this.clientSocket = clientSocket;
		}

		public void StartHandling()
		{
			BasicMetadataPacket packetData = metadataHandler.ReceiveBasicMetadataPacket();
			ReceivePackets(clientSocket, packetData);
		}

		private void ReceivePackets(Socket clientSocket, BasicMetadataPacket packetData)
		{
			string clientAddress = ShareNetUtil.GetRemotePointAddress(clientSocket);
			switch(packetData.PacketType)
			{
				case PacketType.BasicFrame:
					ValidCheck(ReceivedBasicPacket, packetData, (a) => { BasicMetatdataPacketHandling(a); });
					break;
				case PacketType.BasicSecurity:
					ValidCheck(ReceivedBasicSecurityPacket, packetData, (a) => { BasicSecurityMetadataPacketHandling(a); });
					break;
				case PacketType.ExpertSecurity:
					ValidCheck(ReceivedExpertSecurityPacket, packetData, (a) => { ExpertSecurityDataPacketHandling(a); });
					break;
				case PacketType.Info:
					ReceiveInfoPacketHandling(clientSocket);
					break;
			};

			ReceiveEnd(new ServerEventArgs("", clientAddress));
		}

		private void ReceiveInfoPacketHandling(Socket clientSocket)
		{
			InfoPacket info = ShareNetUtil.ReceiveInfoPacket(clientSocket);

			if(info.Info == InfoType.Error)
			{
				ErrorPacket error = ShareNetUtil.ReceiveErrorPacket(clientSocket, info);
				ReceivedErrorPacket(new ServerEventArgs(error.ErrorType.ToString(), ShareNetUtil.GetRemotePointAddress(clientSocket)));
			}
			else
				ReceivedInvaildPacket(new ServerEventArgs(info.Info.ToString(), ShareNetUtil.GetRemotePointAddress(clientSocket)));
		}

		private delegate void ValidCheckDelegate(BasicMetadataPacket data);

		private void ValidCheck(ServerTransferEvent callEvent, BasicMetadataPacket packetData, ValidCheckDelegate work)
		{
			ServerEventArgs eventArg = new ServerEventArgs("", ShareNetUtil.GetRemotePointAddress(clientSocket));
			callEvent(eventArg);

			if(!eventArg.Cancel)
				work(packetData);
			else
				ServerNetUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);
		}

		private void BasicMetatdataPacketHandling (BasicMetadataPacket packetData)
		{
			if(config.GetConfigTable("Accept_Default_Packet") == bool.TrueString)
			{
				ShareNetUtil.SendInfoPacket(clientSocket, InfoType.Accept);

				for(int i = 0; i < packetData.DataCount; i++)
				{ BasicDataPacket data = dataHandler.ReceiveBasicDataPacket(AnonymousFolderName); }

				clientSocket.Send(new InfoPacket(InfoType.Close).GetBinaryData());
				clientSocket.Close(150);
			}
			else
				ServerNetUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);
		}

		private void BasicSecurityMetadataPacketHandling (BasicMetadataPacket packetData)
		{
			if(config.GetConfigTable("Accept_Basic_Security_Packet") == bool.TrueString)
			{
				BasicSecurityMetadataPacket childPacket = metadataHandler.ReceiveBasicSecurityMetadataPacket(packetData);

				if(childPacket.IsAnonynomus == true)
				{
					if(config.GetConfigTable("Accept_Anonymous_Login") == bool.TrueString)
					{
						ShareNetUtil.SendInfoPacket(clientSocket, InfoType.Accept);
						BasicSecurityDataPacektHandling(childPacket);

						clientSocket.Send(new InfoPacket(InfoType.Close).GetBinaryData());
						clientSocket.Close(150);
					}
					else
						ServerNetUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Anonymous);
				}
				else if(Util.GetHashedString(accountConfig.GetConfigTable(childPacket.Username)) == Util.GetHashedString(childPacket.Password))
				{
					ShareNetUtil.SendInfoPacket(clientSocket, InfoType.Accept);
					BasicSecurityDataPacektHandling(childPacket);

					clientSocket.Send(new InfoPacket(InfoType.Close).GetBinaryData());
					clientSocket.Close(150);
				}
				else
					ServerNetUtil.SendErrorPacket(clientSocket, ErrorType.Wrong_Certificate);
			}
			else
				ServerNetUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);
		}

		private void BasicSecurityDataPacektHandling(BasicSecurityMetadataPacket packetData)
		{
			for(int i = 0; i < packetData.DataCount; i++)
			{
				BasicSecurityDataPacket data = dataHandler.ReceiveBasicSecurityDataPacket(packetData.IsAnonynomus ? AnonymousFolderName : packetData.Username);
			}
		}

		private void ExpertSecurityMetadataPacketHandling(BasicMetadataPacket packetData)
		{
			if(config.GetConfigTable("Accpet_Expert_Security_Packet") == bool.TrueString)
			{
				ExpertSecurityMetadataPacket childPacket = metadataHandler.ReceiveExpertSecurityMetadataPacket(packetData);

				if (childPacket.IsAnonynomus == true)
				{
					if(config.GetConfigTable("Accept_Anonymous_Login") == bool.TrueString)
					{
						ExpertSecurityDataPacketHandling(packetData);

						clientSocket.Send(new InfoPacket(InfoType.Close).GetBinaryData());
						clientSocket.Close(150);
					}
					else
						ServerNetUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Anonymous);
				}
				else if(Util.GetHashedString(accountConfig.GetConfigTable(childPacket.Username)) == Util.GetHashedString(childPacket.Password))
				{
					ShareNetUtil.SendInfoPacket(clientSocket, InfoType.Accept);
					ExpertSecurityDataPacketHandling(packetData);

					clientSocket.Send(new InfoPacket(InfoType.Close).GetBinaryData());
					clientSocket.Close(150);
				}
			}
			else
				ServerNetUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);
		}

		private void ExpertSecurityDataPacketHandling(BasicMetadataPacket packetData)
		{
			using(DH521Manager dh = new DH521Manager())
			{
				ShareNetUtil.SendInfoPacket(clientSocket, InfoType.Accept, dh.PublicKey);

				// Receive Client Share Key
				InfoPacket clientShareInfo = ShareNetUtil.ReceiveInfoPacket(clientSocket);

				if(clientShareInfo.ResponseData == null)
					ServerNetUtil.SendErrorPacket(clientSocket, ErrorType.Security_Alert);
				else
				{
					for(int i = 0; i < packetData.DataCount; i++)
					{
						ExpertSecurityDataPacket data = dataHandler.ReceiveExpertSecurityDataPacket(dh.GetShareKey(clientShareInfo.ResponseData), "");
					}
				}
			}
		}
	}
}
