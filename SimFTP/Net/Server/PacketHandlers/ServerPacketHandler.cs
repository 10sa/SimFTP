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
	class ServerPacketHandler
	{
		private readonly AccountConfig accountConfig;
		private readonly TransferConfig config;

		public ServerPacketHandler(Socket clientSocket,ref TransferConfig config, ref AccountConfig accountConfig)
		{
			this.config = config;
			this.accountConfig = accountConfig;
		}

		

		#region Data Packets Receive
		private static BasicDataPacket ReceiveBasicDataPacket (Socket clientSocket)
		{
			short fileNameLenght = BitConverter.ToInt16(ReceivePacket(clientSocket, sizeof(short)), 0);
			long fileSize = BitConverter.ToInt64(ReceivePacket(clientSocket, sizeof(long)), 0);
			string fileName = Encoding.UTF8.GetString(ReceivePacket(clientSocket, fileNameLenght));
			byte[] fileData = GetFileData(clientSocket, fileSize);

			return new BasicDataPacket(fileNameLenght, fileSize, fileName, fileData);
		}

		private static BasicSecurityDataPacket ReceiveBasicSecurityDataPacket (Socket clientSocket)
		{
			BasicDataPacket data = ReceiveBasicDataPacket(clientSocket);
			return new BasicSecurityDataPacket(data.FileName, data.FileNameLenght, data.FileData, data.FileSize, ReceivePacket(clientSocket, Util.HashByteSize));
		}

		private static byte[] GetFileData (Socket clientSocket, long fileSize)
		{
			byte[] fileData;

			if(fileSize > int.MaxValue)
			{
				fileData = ReceivePacket(clientSocket, int.MaxValue);
				for(long i = (fileSize - int.MaxValue); fileSize > int.MaxValue; fileSize -= int.MaxValue)
					fileData = Util.AttachByteArray(fileData, ReceivePacket(clientSocket, int.MaxValue));

				fileData = Util.AttachByteArray(fileData, ReceivePacket(clientSocket, (int)fileSize));
			}
			else
				fileData = ReceivePacket(clientSocket, (int)fileSize);

			return fileData;
		}
		#endregion

		#region Server Util

		#endregion
	}
}
