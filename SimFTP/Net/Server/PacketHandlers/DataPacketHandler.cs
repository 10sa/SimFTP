using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using SimFTP.Security;
using SimFTP.Net.DataPackets;

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
		private bool isUseUserFolder = false;
		private string usernameFolder;

		public DataPacketHandler(Socket clientSocket, string saveFolder, bool isSaveDateFolder, bool isOverwrite, bool isUseUserFolder)
		{
			this.clientSocket = clientSocket;
			this.saveFolder = saveFolder;
			this.isSaveInDateFolder = isSaveDateFolder;
			this.isOverwrite = isOverwrite;
			this.isUseUserFolder = isUseUserFolder;
		}

		public BasicDataPacket ReceiveBasicDataPacket (string usernameFolder)
		{
			short fileNameLength = BitConverter.ToInt16(ShareNetUtil.ReceivePacket(clientSocket, sizeof(short)), 0);
			long fileSize = BitConverter.ToInt64(ShareNetUtil.ReceivePacket(clientSocket, sizeof(long)), 0);
			string fileName = Encoding.UTF8.GetString(ShareNetUtil.ReceivePacket(clientSocket, fileNameLength));
			this.usernameFolder = usernameFolder;

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
				string fileName = GetLastFileName(data.FileName);

				using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
				{
					CryptoStream writerStream = aes.GetDencryptStream(File.Open(fileName + ".DENCRYPT", FileMode.Create));

					byte[] buffer = new byte[int.MaxValue / 8];
					int readedBytes;

					while((readedBytes = reader.Read(buffer, 0, buffer.Length)) > 0)
						writerStream.Write(buffer, 0, readedBytes);

					writerStream.Dispose();
				}

				File.Delete(fileName);
				File.Move(fileName + ".DENCRYPT", fileName);

				return data;
			}
		}

		private void GetFileData (Socket clientSocket, long fileSize, string fileName)
		{
			const int FileBufferSize = int.MaxValue / 8;
			long leftFileSize = fileSize;

			using(BinaryWriter writer = new BinaryWriter(File.Create(GetLastFileName(fileName, false), FileBufferSize)))
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

		private string GetLastFileName(string fileName, bool getExistFile = true)
		{
			StringBuilder builder = new StringBuilder();
			string fileFormat = CreateBasicFilepath(builder, fileName);

			if (!File.Exists(builder.ToString() + string.Format(" (1)." + fileFormat)))
			{
				if (getExistFile)
					return builder.ToString() + "." + fileFormat;
				else
				{
					if (!File.Exists(builder.ToString() + "." + fileFormat))
						return builder.ToString() + "." + fileFormat;
					else
						return builder.ToString() + string.Format(" (1)." + fileFormat);
				}
			}

			for(int ctl = 1; ; ctl++)
			{
				if (!File.Exists(builder.ToString() + string.Format(" ({0}).{1}", ctl + 1, fileFormat)))
				{
					if (getExistFile)
						return builder.ToString() + string.Format(" ({0}).{1}", ctl, fileFormat);
					else
						return builder.ToString() + string.Format(" ({0}).{1}", ctl + 1, fileFormat);
				}
			}
		}

		private string CreateBasicFilepath(StringBuilder builder, string fileName)
		{
			if (saveFolder != string.Empty)
				builder.Append(saveFolder + "/");
			else
				builder.Append(Environment.CurrentDirectory + "/");

			if (isSaveInDateFolder)
				builder.Append(DateTime.Now.ToString("yyyy_MM_dd") + "/");

			if (isUseUserFolder)
				builder.Append(usernameFolder + "/");

			if (!Directory.Exists(builder.ToString()))
				Directory.CreateDirectory(builder.ToString());

			string[] splitName = fileName.Split('.');

			for (int i = 0; i < splitName.Length - 1; i++)
				builder.Append(splitName[i]);

			return splitName.Last();
		}
	}
}
