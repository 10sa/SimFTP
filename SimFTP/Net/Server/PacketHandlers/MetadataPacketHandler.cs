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
using SimFTP.Net.Server;
using SimFTP.Security;

using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace SimFTP.Net.Server.PacketHandlers
{
	class MetadataPacketHandler
	{
		private Socket clientSocket;

		public MetadataPacketHandler(Socket clientSocket)
		{
			this.clientSocket = clientSocket;
		}

		#region Metadata Packets Receive
		public BasicMetadataPacket ReceiveBasicMetadataPacket()
		{
			PacketType packetType = ShareNetUtil.GetPacketType(clientSocket);
			int dataCount = BitConverter.ToInt32(ShareNetUtil.ReceivePacket(clientSocket, sizeof(int)), 0);

			return new BasicMetadataPacket(dataCount, packetType);
		}

		public BasicSecurityMetadataPacket ReceiveBasicSecurityMetadataPacket(BasicMetadataPacket orignalPacket)
		{
			bool isAnonymous = BitConverter.ToBoolean(ShareNetUtil.ReceivePacket(clientSocket, sizeof(bool)), 0);

			if(isAnonymous == true)
				return new BasicSecurityMetadataPacket(orignalPacket.DataCount);
			else
			{
				short usernameLenght = BitConverter.ToInt16(ShareNetUtil.ReceivePacket(clientSocket, sizeof(short)), 0);
				short passwordLenght = BitConverter.ToInt16(ShareNetUtil.ReceivePacket(clientSocket, sizeof(short)), 0);
				string username = Encoding.UTF8.GetString(ShareNetUtil.ReceivePacket(clientSocket, usernameLenght));
				string password = Encoding.UTF8.GetString(ShareNetUtil.ReceivePacket(clientSocket, passwordLenght));

				return new BasicSecurityMetadataPacket(username, password, orignalPacket.DataCount);
			}
		}

		public ExpertSecurityMetadataPacket ReceiveExpertSecurityMetadataPacket(BasicMetadataPacket orignalPacket)
		{
			return new ExpertSecurityMetadataPacket(ReceiveBasicSecurityMetadataPacket(orignalPacket), ShareNetUtil.ReceivePacket(clientSocket, DH521Manager.PublicKeySize));
		}
		#endregion
	}
}
