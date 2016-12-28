using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

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
			byte[][] argArray = { BitConverter.GetBytes(FileNameLenght), BitConverter.GetBytes(FileSize), Encoding.UTF8.GetBytes(FileName), FileData };
			byte[] buffer = new byte[sizeof(short) + sizeof(long) + GetByteCount(FileName) + FileData.Length];

			long dataLength = 0;
			foreach (var arg in argArray)
				dataLength = BulidArray(arg, buffer, dataLength);

			return buffer;
		}

		protected long BulidArray(byte[] soruceBuffer, byte[] orignalBuffer, long length)
		{
			Array.Copy(soruceBuffer, 0, orignalBuffer, length, soruceBuffer.LongLength);
			return length + soruceBuffer.LongLength;
		}

		protected int GetByteCount(string s)
		{
			return Encoding.UTF8.GetByteCount(s);
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
			using (SHA256CryptoServiceProvider hash = new SHA256CryptoServiceProvider())
			{
				Checksum = hash.ComputeHash(fileData);
			}
		}

		public new byte[] GetBinaryData()
		{
			byte[][] argsArray = { base.GetBinaryData(), Checksum };
			byte[] buffer = new byte[argsArray[0].LongLength + Checksum.LongLength];

			long dataLenght = 0;
			foreach(var arg in argsArray)
				dataLenght = BulidArray(arg, buffer, dataLenght);

			return buffer;
		}
	}
}
