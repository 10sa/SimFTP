using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimFTP;
using SimFTP.Config;

using SimFTP.Net;
using SimFTP.Security;
using SimFTP.Net.DataPackets;
using SimFTP.Net.MetadataPackets;

using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace SimFTP.Net.Server.PacketHandlers
{
	class DataPacketHandler
	{
		private Socket clientSocket;

		public DataPacketHandler(Socket clientSocket)
		{
			this.clientSocket = clientSocket;	
		}

		public BasicDataPacket ReceiveBasicDataPacket ()
		{
			short fileNameLenght = BitConverter.ToInt16(ShareNetUtil.ReceivePacket(clientSocket, sizeof(short)), 0);
			long fileSize = BitConverter.ToInt64(ShareNetUtil.ReceivePacket(clientSocket, sizeof(long)), 0);
			string fileName = Encoding.UTF8.GetString(ShareNetUtil.ReceivePacket(clientSocket, fileNameLenght));
			byte[] fileData = GetFileData(clientSocket, fileSize);

			return new BasicDataPacket(fileNameLenght, fileSize, fileName, fileData);
		}

		public BasicSecurityDataPacket ReceiveBasicSecurityDataPacket ()
		{
			BasicDataPacket data = ReceiveBasicDataPacket();
			return new BasicSecurityDataPacket(data.FileName, data.FileNameLenght, data.FileData, data.FileSize, ShareNetUtil.ReceivePacket(clientSocket, Util.HashByteSize));
		}

		public ExpertSecurityDataPacket ReceiveExpertSecurityDataPacket(byte[] shareKey)
		{
			using(AES256Manager aes = new AES256Manager(shareKey))
			{
				ExpertSecurityDataPacket data = new ExpertSecurityDataPacket(ReceiveBasicSecurityDataPacket());
				data.SetFileData(aes.Decrypt(data.FileData));

				return data;
			}
		}

		private byte[] GetFileData (Socket clientSocket, long fileSize)
		{
			byte[] fileData;

			if(fileSize > int.MaxValue)
			{
				fileData = ShareNetUtil.ReceivePacket(clientSocket, int.MaxValue);
				for(long i = (fileSize - int.MaxValue); fileSize > int.MaxValue; fileSize -= int.MaxValue)
					fileData = Util.AttachByteArray(fileData, ShareNetUtil.ReceivePacket(clientSocket, int.MaxValue));

				fileData = Util.AttachByteArray(fileData, ShareNetUtil.ReceivePacket(clientSocket, (int)fileSize));
			}
			else
				fileData = ShareNetUtil.ReceivePacket(clientSocket, (int)fileSize);

			return fileData;
		}
	}
}
