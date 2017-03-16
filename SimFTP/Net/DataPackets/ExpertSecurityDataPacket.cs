using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimFTP.Net.DataPackets
{
	public class ExpertSecurityDataPacket : BasicSecurityDataPacket
	{
		public ExpertSecurityDataPacket(BasicDataPacket data) : base(data) { }
		
		public void SetFileData(byte[] fileData)
		{
			// Override //
			this.FileData = fileData;
			this.FileSize = FileData.LongLength;
		}
	}
}
