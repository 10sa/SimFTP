using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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

		protected BasicSecurityDataPacket(BasicDataPacket data) : base(data)
		{
			SetChecksum(data.FileData);
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
