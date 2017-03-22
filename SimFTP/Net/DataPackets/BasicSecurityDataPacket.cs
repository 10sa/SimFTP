using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace SimFTP.Net.DataPackets
{
	public class BasicSecurityDataPacket : BasicDataPacket
	{
		public byte[] Checksum { get; private set; }

		public BasicSecurityDataPacket(string fileName, short nameLenght, byte[] fileData, long fileSize) : base(nameLenght, fileSize, fileName, fileData)
		{
			SetChecksum(fileData);
		}

		public BasicSecurityDataPacket(string fileName, short nameLenght, byte[] fileData, long fileSize, byte[] checksum) : base(nameLenght, fileSize, fileName, fileData)
		{
			Checksum = checksum;
		}

		public BasicSecurityDataPacket(string fileName, short nameLenght, FileStream stream) : base(nameLenght, fileName, stream) 
		{
			SetChecksum(stream);
		}

		protected BasicSecurityDataPacket(BasicDataPacket data) : base(data)
		{
			if(data.File != null)
				SetChecksum(data.File);
			else if(data.FileData != null)
				SetChecksum(data.FileData);
		}

		private void SetChecksum(byte[] fileData)
		{
			Checksum = Util.GetHashValue(fileData);
		}

		private void SetChecksum(FileStream stream)
		{
			Checksum = Util.GetHashFromStream(stream);
		}

		public new byte[] GetBinaryData()
		{
			return Util.AttachByteArray(base.GetBinaryData(), Checksum);
		}
	}
}
