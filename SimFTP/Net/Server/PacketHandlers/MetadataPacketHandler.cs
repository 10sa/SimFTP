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
	class MetadataPacketHandler
	{
		private void BasicMetatdataPacketHandling (Socket clientSocket, BasicMetadataPacket packetData)
		{
			if(config.GetConfigTable("Accept_Default_Packet") == bool.TrueString)
			{
				for(int i = 0; i < packetData.DataCount; i++)
				{
					BasicDataPacket data = ReceiveBasicDataPacket(clientSocket);
					Util.WriteFile(data.FileData, data.FileName);
				}
			}
			else
				SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);
		}

		private void BasicSecurityMetadataPacketHandling (Socket clientSocket, BasicMetadataPacket packetData)
		{
			if(config.GetConfigTable("Accept_Basic_Security_Packet") == bool.TrueString)
			{
				BasicSecurityMetadataPacket childPacket = ReceiveBasicSecurityMetadataPacket(clientSocket, packetData);

				if(childPacket.IsAnonynomus == true)
				{
					if(config.GetConfigTable("Accept_Anonymous_Login") == bool.TrueString)
					{
						BasicSecurityDataPacket data = ReceiveBasicSecurityDataPacket(clientSocket);
						Util.WriteFile(data.FileData, data.FileName);
					}
					else
						SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Anonymous);
				}
				else if(accountConfig.GetConfigTable(childPacket.Username) == Util.GetHashedString(childPacket.Password))
				{
					BasicSecurityDataPacket data = ReceiveBasicSecurityDataPacket(clientSocket);
					Util.WriteFile(data.FileData, data.FileName);
				}
				else
					SendErrorPacket(clientSocket, ErrorType.Wrong_Certificate);
			}
			else
				SendErrorPacket(clientSocket, ErrorType.Not_Accepted_Packet);
		}

		#region Metadata Packets Receive
		private static BasicMetadataPacket ReceiveBasicMetadataPacket (Socket clientSocket)
		{
			PacketType packetType = GetPacketType(clientSocket);
			int dataCount = BitConverter.ToInt32(ReceivePacket(clientSocket, sizeof(int)), 0);

			return new BasicMetadataPacket(dataCount);
		}

		private static BasicSecurityMetadataPacket ReceiveBasicSecurityMetadataPacket (Socket clientSocket, BasicMetadataPacket orignalPacket)
		{
			bool isAnonymous = BitConverter.ToBoolean(ReceivePacket(clientSocket, sizeof(bool)), 0);

			if(isAnonymous == true)
				return new BasicSecurityMetadataPacket(orignalPacket.DataCount);
			else
			{
				short usernameLenght = BitConverter.ToInt16(ReceivePacket(clientSocket, sizeof(short)), 0);
				short passwordLenght = BitConverter.ToInt16(ReceivePacket(clientSocket, sizeof(short)), 0);
				string username = Encoding.UTF8.GetString(ReceivePacket(clientSocket, usernameLenght));
				string password = Encoding.UTF8.GetString(ReceivePacket(clientSocket, passwordLenght));

				return new BasicSecurityMetadataPacket(username, password, orignalPacket.DataCount);
			}
		}
		#endregion
	}
}
