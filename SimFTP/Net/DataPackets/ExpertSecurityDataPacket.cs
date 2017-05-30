using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimFTP.Net.MetadataPackets;

namespace SimFTP.Net.DataPackets
{
	public class ExpertSecurityDataPacket : BasicSecurityDataPacket
	{
		public ExpertSecurityDataPacket(BasicDataPacket data) : base(data) { }

		public override BasicMetadataPacket CreateMetadata(int dataCount)
		{
			return new ExpertSecurityMetadataPacket(dataCount, null);
		}

		public void SetFileSize(long fileSize)
		{
			if(fileSize < 0)
				throw new ArgumentException();

			this.FileSize = fileSize;
		}
	}
}
