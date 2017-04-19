using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


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
		private string saveFolder;
		private bool isSaveInDateFolder = false;
		private bool isOverwrite = false;
		private bool isUsePlusFolder = false;
		private string plusFolder;

		public DataPacketHandler(Socket clientSocket, string saveFolder, bool isSaveDateFolder, bool isOverwrite, bool isUsePlusFolder)
		{
			this.clientSocket = clientSocket;
			this.saveFolder = saveFolder;
			this.isSaveInDateFolder = isSaveDateFolder;
			this.isOverwrite = isOverwrite;
			this.isUsePlusFolder = isUsePlusFolder;
		}

		public BasicDataPacket ReceiveBasicDataPacket (string plusFolder)
		{
			short fileNameLength = BitConverter.ToInt16(ShareNetUtil.ReceivePacket(clientSocket, sizeof(short)), 0);
			long fileSize = BitConverter.ToInt64(ShareNetUtil.ReceivePacket(clientSocket, sizeof(long)), 0);
			string fileName = Encoding.UTF8.GetString(ShareNetUtil.ReceivePacket(clientSocket, fileNameLength));
			this.plusFolder = plusFolder;

			GetFileData(clientSocket, fileSize, fileName);

			return new BasicDataPacket(fileName, null);
		}

		public BasicSecurityDataPacket ReceiveBasicSecurityDataPacket (string plusFolder)
		{
			BasicDataPacket data = ReceiveBasicDataPacket(plusFolder);
			return new BasicSecurityDataPacket(data.FileName,  ShareNetUtil.ReceivePacket(clientSocket, Util.HashByteSize));
		}

		public ExpertSecurityDataPacket ReceiveExpertSecurityDataPacket(byte[] shareKey, string plusFolder)
		{
			using(AES256Manager aes = new AES256Manager(shareKey))
			{
				ExpertSecurityDataPacket data = new ExpertSecurityDataPacket(ReceiveBasicSecurityDataPacket(plusFolder));

				using(BinaryReader reader = new BinaryReader(File.Open(data.FileName, FileMode.Open)))
				{
					CryptoStream writerStream = aes.GetDencryptStream(File.Open(data.FileName + ".DENCRYPT", FileMode.Create));

					byte[] buffer = new byte[int.MaxValue / 8];
					int readedBytes;

					while((readedBytes = reader.Read(buffer, 0, buffer.Length)) > 0)
						writerStream.Write(buffer, 0, readedBytes);

					writerStream.Dispose();
				}

				File.Delete(data.FileName);
				File.Move(data.FileName + ".DENCRYPT", data.FileName);

				return data;
			}
		}

		private void GetFileData (Socket clientSocket, long fileSize, string fileName)
		{
			const int FileBufferSize = int.MaxValue / 8;
			StringBuilder builder = new StringBuilder();
			long leftFileSize = fileSize;
			CreateFileName(fileName, builder);

			using(BinaryWriter writer = new BinaryWriter(File.Create(builder.ToString(), FileBufferSize)))
			{
				byte[] buffer = new byte[FileBufferSize];
				while(leftFileSize > buffer.Length)
				{
					int readedSize = ShareNetUtil.BufferedReceivePacket(clientSocket, buffer, buffer.Length);
					writer.Write(buffer, 0, readedSize);
					leftFileSize -= readedSize;
				}

				while(leftFileSize > 0)
				{
					int readedSize = ShareNetUtil.BufferedReceivePacket(clientSocket, buffer, (int)leftFileSize);
					writer.Write(buffer, 0, readedSize);
					leftFileSize -= readedSize;
				}

				buffer = null;
				writer.Close();
			}

			GC.Collect();
		}

		private void CreateFileName(string fileName, StringBuilder builder)
		{
			if(saveFolder != string.Empty)
				builder.Append(saveFolder);
			else
				builder.Append(Environment.CurrentDirectory + "/");

			if(isSaveInDateFolder)
				builder.Append(DateTime.Now.ToString("yyyy_MM_dd") + "/");

			if(isUsePlusFolder)
				builder.Append(plusFolder + "/");

			if(!Directory.Exists(builder.ToString()))
				Directory.CreateDirectory(builder.ToString());

			string[] splitName = fileName.Split('.');
			for(int i = 0; i < splitName.Length - 1; i++)
				builder.Append(splitName[i]);

			if(!isOverwrite)
			{
				string name = builder.ToString();
				for(int i = 1; ; i++)
				{
					if(!File.Exists(string.Format(name + " ({0})", i)))
						builder.Append(string.Format(" ({0}).{1}", i, splitName.Last()));
				}
			}
		}
	}
}
