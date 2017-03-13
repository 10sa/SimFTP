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
		private readonly AccountConfig accountConfig;
		private readonly TransferConfig config;

		private MetadataPacketHandler metadataHandler;
		private DataPacketHandler dataHandler;
		private Socket clientSocket;

		public ServerPacketHandler(Socket clientSocket, ref TransferConfig config, ref AccountConfig accountConfig)
		{
			this.config = config;
			this.accountConfig = accountConfig;

			metadataHandler = new MetadataPacketHandler(clientSocket);
			dataHandler = new DataPacketHandler(clientSocket);
			this.clientSocket = clientSocket;

			BasicMetadataPacket packetData = metadataHandler.ReceiveBasicMetadataPacket();

			switch(packetData.PacketType)
			{
				case PacketType.BasicFrame:
					BasicMetatdataPacketHandling(packetData);
					break;
				case PacketType.BasicSecurity:
					BasicSecurityMetadataPacketHandling(packetData);
					break;
				case PacketType.ExpertSecurity:
					break;
				case PacketType.Error:
					break;
			};
		}

		private void BasicMetatdataPacketHandling (BasicMetadataPacket packetData)
		{
			if(config.GetConfigTable("Accept_Default_Packet") == bool.TrueString)
			{
				ServerUtil.SendInfoPacket(clientSocket, InfoType.Accept);

				for(int i = 0; i < packetData.DataCount; i++)
				{
					BasicDataPacket data = dataHandler.ReceiveBasicDataPacket();
					Util.WriteFile(data.FileData, data.FileName);
				}
			}
			else
				ServerUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);
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
						ServerUtil.SendInfoPacket(clientSocket, InfoType.Accept);
						BasicSecurityDataPacektHandling(packetData);
					}
					else
						ServerUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Anonymous);
				}
				else if(accountConfig.GetConfigTable(childPacket.Username) == Util.GetHashedString(childPacket.Password))
				{
					ServerUtil.SendInfoPacket(clientSocket, InfoType.Accept);
					BasicSecurityDataPacektHandling(packetData);
				}
				else
					ServerUtil.SendErrorPacket(clientSocket, ErrorType.Wrong_Certificate);
			}
			else
				ServerUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);
		}

		private void BasicSecurityDataPacektHandling(BasicMetadataPacket packetData)
		{
			for(int i = 0; i < packetData.DataCount; i++)
			{
				BasicSecurityDataPacket data = dataHandler.ReceiveBasicSecurityDataPacket();
				Util.WriteFile(data.FileData, data.FileName);
			}
		}

		private void ExpertSecurityMetadataPacketHandling(BasicMetadataPacket packetData)
		{
			if(config.GetConfigTable("Accpet_Expert_Security_Packet") == bool.TrueString)
			{
				ExpertSecurityMetadataPacket childPacket = metadataHandler.ReceiveExpertSecurityMetadataPacket(packetData);

				if (childPacket.IsAnonynomus == true)
				{
					if(config.GetConfigTable("Accpet_Anonymous_Login") == bool.TrueString)
						ExpertSecurityDataPacketHandling(packetData);
					else
						ServerUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Anonymous);
				}
			}
			else
				ServerUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);
		}

		private void ExpertSecurityDataPacketHandling(BasicMetadataPacket packetData)
		{
			using(DH521Manager dh = new DH521Manager())
			{
				ServerUtil.SendInfoPacket(clientSocket, InfoType.Accept, dh.PublicKey);

				// Receive Client Share Key
				BasicMetadataPacket shareKeyPacket = metadataHandler.ReceiveBasicMetadataPacket();
				int ResponseLenght = BitConverter.ToInt32(ServerUtil.ReceivePacket(clientSocket, sizeof(int)), 0);

				if(ResponseLenght <= 0)
					ServerUtil.SendErrorPacket(clientSocket, ErrorType.Security_Alert);
				else
				{
					byte[] clientPublicKey = ServerUtil.ReceivePacket(clientSocket, ResponseLenght);
					for(int i = 0; i < packetData.DataCount; i++)
					{
						ExpertSecurityDataPacket data = dataHandler.ReceiveExpertSecurityDataPacket(dh.GetShareKey(clientPublicKey));
						Util.WriteFile(data.FileData, data.FileName);
					}
				}
			}
		}
	}
}
