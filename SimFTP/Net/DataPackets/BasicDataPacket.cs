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

		public FileStream File { get; protected set; }

		private bool _CalledOnlyBasicDataPacket = false;

		public bool IsOversize { get { if (FileSize > int.MaxValue / 4) { return true; } else { return false; } } }

		private BasicDataPacket () { }

		public BasicDataPacket(string fileName, FileStream file)
		{
			FileNameLenght = (short)fileName.Length;
			FileName = fileName;
			File = file;

			if(file != null)
				FileSize = file.Length;
		}

		public byte[] GetBinaryData()
		{
			if (!_CalledOnlyBasicDataPacket)
				return Util.AttachByteArray(BitConverter.GetBytes(FileNameLenght), BitConverter.GetBytes(FileSize), Encoding.UTF8.GetBytes(FileName));
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
			SetFileData(data.FileNameLenght, data.FileSize, data.FileName, data.File);
		}

		private void SetFileData(short nameLenght, long fileSize, string fileName, FileStream stream)
		{
			this.FileNameLenght = nameLenght;
			this.FileSize = fileSize;
			this.FileName = fileName;
			this.File = stream;
		}		
	}
}
