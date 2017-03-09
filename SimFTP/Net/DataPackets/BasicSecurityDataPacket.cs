using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Simple_File_Transfer.Net.DataPackets
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
