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
						BasicSecurityDataPacket data = dataHandler.ReceiveBasicSecurityDataPacket();
						Util.WriteFile(data.FileData, data.FileName);
					}
					else
						ServerUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Anonymous);
				}
				else if(accountConfig.GetConfigTable(childPacket.Username) == Util.GetHashedString(childPacket.Password))
				{
					BasicSecurityDataPacket data = dataHandler.ReceiveBasicSecurityDataPacket();
					Util.WriteFile(data.FileData, data.FileName);
				}
				else
					ServerUtil.SendErrorPacket(clientSocket, ErrorType.Wrong_Certificate);
			}
			else
				ServerUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);
		}
	}
}
