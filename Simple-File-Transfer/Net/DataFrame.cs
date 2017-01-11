using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Simple_File_Transfer;

namespace Simple_File_Transfer.Net
{
	public class DataFrame
	{
		public short FileNameLenght { get; private set; }
		public long FileSize { get; private set; }

		public string FileName { get; private set; }
		public byte[] FileData { get; private set; }

		private DataFrame() { }

		public DataFrame(short nameLenght, long fileSize, string fileName, byte[] fileData)
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

	public class BasicSecurityDataFrame : DataFrame
	{
		public byte[] Checksum { get; private set; }

		public BasicSecurityDataFrame(string fileName, short nameLenght, byte[] fileData, long fileSize) : base(nameLenght, fileSize, fileName, fileData)
		{
			SetChecksum(fileData);
			return;
		}

		private void SetChecksum(byte[] fileData)
		{
			Checksum = Util.GetHashValue(fileData);
		}

		public new byte[] GetBinaryData()
		{
			return Util.AttachByteArray(base.GetBinaryData(), Checksum);
		}
	}
}
