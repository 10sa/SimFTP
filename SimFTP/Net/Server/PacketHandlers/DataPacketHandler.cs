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
using System.IO;

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
			byte[] fileData = GetFileData(clientSocket, fileSize, fileName);

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

		private byte[] GetFileData (Socket clientSocket, long fileSize, string fileName)
		{
			byte[] fileData;
			const int FileBufferSize = int.MaxValue / 8;
			long readedFileSize = fileSize;

			if(fileSize > FileBufferSize)
			{
				using(BinaryWriter writer = new BinaryWriter(File.Create(fileName, FileBufferSize)))
				{
					for(; readedFileSize > FileBufferSize; readedFileSize -= FileBufferSize)
					{
						if (readedFileSize >= FileBufferSize)
						{
							fileData = ShareNetUtil.ReceivePacket(clientSocket, FileBufferSize);
							writer.Write(fileData, 0, fileData.Length);
						}
					}

					fileData = ShareNetUtil.ReceivePacket(clientSocket, (int)readedFileSize);
					writer.Write(fileData, 0, (int)readedFileSize);
				}

				return null;
			}
			else
				return ShareNetUtil.ReceivePacket(clientSocket, (int)fileSize);
		}
	}
}
