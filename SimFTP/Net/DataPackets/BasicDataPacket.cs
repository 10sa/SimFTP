using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Simple_File_Transfer;


namespace Simple_File_Transfer.Net.DataPackets
{
	public class BasicDataPacket 
	{
		public short FileNameLenght { get; private set; }
		public long FileSize { get; private set; }

		public string FileName { get; private set; }
		public byte[] FileData { get; private set; }

		private BasicDataPacket () { }

		public BasicDataPacket (short nameLenght, long fileSize, string fileName, byte[] fileData)
		{
			this.FileNameLenght = nameLenght;
			this.FileSize = fileSize;
			this.FileName = fileName;
			this.FileData = fileData;

			return;
		}

		public byte[] GetBinaryData()
		{
			return Util.AttachByteArray(BitConverter.GetBytes(FileNameLenght), BitConverter.GetBytes(FileSize), Encoding.UTF8.GetBytes(FileName), FileData);
		}
	}
}
