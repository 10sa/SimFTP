using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using SimFTP;


namespace SimFTP.Net.DataPackets
{
	public class BasicDataPacket 
	{
		public short FileNameLenght { get; protected set; }
		public long FileSize { get; protected set; }

		public string FileName { get; protected set; }
		public byte[] FileData { get; protected set; }

		private BasicDataPacket () { }

		public BasicDataPacket(short nameLenght, long fileSize, string fileName, byte[] fileData)
		{
			SetFileData(nameLenght, fileSize, fileName, fileData);
		}
		public byte[] GetBinaryData()
		{
			return Util.AttachByteArray(BitConverter.GetBytes(FileNameLenght), BitConverter.GetBytes(FileSize), Encoding.UTF8.GetBytes(FileName), FileData);
		}

		protected BasicDataPacket(BasicDataPacket data)
		{
			SetFileData(data.FileNameLenght, data.FileSize, data.FileName, data.FileData);
		}

		private void SetFileData(short nameLenght, long fileSize, string fileName, byte[] fileData)
		{
			this.FileNameLenght = nameLenght;
			this.FileSize = fileSize;
			this.FileName = fileName;
			this.FileData = fileData;
		}		
	}
}
