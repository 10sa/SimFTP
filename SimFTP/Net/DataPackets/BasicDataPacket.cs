using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Net.Sockets;

using SimFTP;


namespace SimFTP.Net.DataPackets
{
	public class BasicDataPacket 
	{
		public short FileNameLenght { get; protected set; }
		public long FileSize { get; protected set; }

		public string FileName { get; protected set; }
		public byte[] FileData { get; protected set; }

		public FileStream File { get; protected set; }

		private bool _CalledOnlyBasicDataPacket = false;

		private BasicDataPacket () { }

		public BasicDataPacket(short nameLenght, long fileSize, string fileName, byte[] fileData)
		{
			SetFileData(nameLenght, fileSize, fileName, fileData);
		}

		public BasicDataPacket(short nameLenght, string fileName, FileStream file)
		{
			FileNameLenght = nameLenght;
			FileName = fileName;
			File = file;
		}

		public byte[] GetBinaryData()
		{
			if (FileData == null && !_CalledOnlyBasicDataPacket)
				return Util.AttachByteArray(BitConverter.GetBytes(FileNameLenght), BitConverter.GetBytes(FileSize), Encoding.UTF8.GetBytes(FileName), FileData);
			else
				return new byte[] { };
		}	

		public byte[] GetOnlyBasicDataPacket()
		{
			this._CalledOnlyBasicDataPacket = true;
			return Util.AttachByteArray(BitConverter.GetBytes(FileNameLenght), BitConverter.GetBytes(FileSize), Encoding.UTF8.GetBytes(FileName));
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
