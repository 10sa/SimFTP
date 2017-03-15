﻿using System;
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
				ShareNetUtil.SendInfoPacket(clientSocket, InfoType.Accept);

				for(int i = 0; i < packetData.DataCount; i++)
				{
					BasicDataPacket data = dataHandler.ReceiveBasicDataPacket();
					Util.WriteFile(data.FileData, data.FileName);
				}

				InfoPacket info = ShareNetUtil.ReceiveInfoPacket(clientSocket);
				clientSocket.Close();
			}
			else
				ServerNetUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);
		}

		private void BasicSecurityMetadataPacketHandling (BasicMetadataPacket packetData)
		{
			clientSocket.Send(new ErrorPacket(ErrorType.Security_Alert).GetBinaryData());
		/*
			if(config.GetConfigTable("Accept_Basic_Security_Packet") == bool.TrueString)
			{
				BasicSecurityMetadataPacket childPacket = metadataHandler.ReceiveBasicSecurityMetadataPacket(packetData);

				if(childPacket.IsAnonynomus == true)
				{
					if(config.GetConfigTable("Accept_Anonymous_Login") == bool.TrueString)
					{
						ShareNetUtil.SendInfoPacket(clientSocket, InfoType.Accept);
						BasicSecurityDataPacektHandling(packetData);
					}
					else
						ServerNetUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Anonymous);
				}
				else if(accountConfig.GetConfigTable(childPacket.Username) == Util.GetHashedString(childPacket.Password))
				{
					ShareNetUtil.SendInfoPacket(clientSocket, InfoType.Accept);
					BasicSecurityDataPacektHandling(packetData);
				}
				else
					ServerNetUtil.SendErrorPacket(clientSocket, ErrorType.Wrong_Certificate);
			}
			else
				ServerNetUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);
			*/
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
						ServerNetUtil.SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Anonymous);
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
				InfoPacket clientPulbicKey = ShareNetUtil.ReceiveInfoPacket(clientSocket);
				int ResponseLenght = BitConverter.ToInt32(ShareNetUtil.ReceivePacket(clientSocket, sizeof(int)), 0);

				if(ResponseLenght <= 0)
					ServerNetUtil.SendErrorPacket(clientSocket, ErrorType.Security_Alert);
				else
				{
					byte[] clientPublicKey = ShareNetUtil.ReceivePacket(clientSocket, ResponseLenght);
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
