using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Net.Sockets;

using SimFTP;

using SimFTP.Net.MetadataPackets;

namespace SimFTP.Net.DataPackets
{
	public class BasicDataPacket 
	{
		public short FileNameLength { get; protected set; }
		public long FileSize { get; protected set; }

		public string FileName { get; protected set; }

		public FileStream File { get; protected set; }

		private bool _CalledOnlyBasicDataPacket = false;

		public virtual BasicMetadataPacket CreateMetadata(int dataCount)
		{
			return new BasicMetadataPacket(dataCount);
		}

		private BasicDataPacket () { }

		public BasicDataPacket(string fileName, FileStream file)
		{
			SetFileData(fileName, file);
		}

		public byte[] GetBinaryData()
		{
			if (!_CalledOnlyBasicDataPacket)
				return Util.AttachByteArray(BitConverter.GetBytes(FileNameLength), BitConverter.GetBytes(FileSize), Encoding.UTF8.GetBytes(FileName));
			else
				return new byte[] { };
		}	

		public byte[] GetOnlyBasicDataPacket()
		{
			this._CalledOnlyBasicDataPacket = true;
			return Util.AttachByteArray(BitConverter.GetBytes(FileNameLength), BitConverter.GetBytes(FileSize), Encoding.UTF8.GetBytes(FileName));
		}

		protected BasicDataPacket(BasicDataPacket data)
		{
			SetFileData(data.FileName, data.File);
		}

		private void SetFileData(string fileName, FileStream stream)
		{
			this.FileNameLength = (short)Encoding.UTF8.GetByteCount(fileName);
			this.FileName = fileName;
			this.File = stream;

			if(stream != null)
				this.FileSize = stream.Length;
		}		
	}
}
