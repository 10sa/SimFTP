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

		public BasicSecurityDataPacket(string fileName, FileStream stream) : base(fileName, stream) 
		{
			SetChecksum(stream);
		}

		public BasicSecurityDataPacket(string fileName, byte[] checksum) : base(fileName, null)
		{
			Checksum = checksum;
		}

		protected BasicSecurityDataPacket(BasicDataPacket data) : base(data)
		{
			if(data.File != null)
				SetChecksum(data.File);
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
