using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimFTP.Net.MetadataPackets;

namespace SimFTP.Net
{
	public enum InfoType
	{
		Response,
		Accept,
		ShareKey,
		Close,
		Error
	}

	public class InfoPacket : BasicMetadataPacket
	{
		public byte[] ResponseData { get; private set; }

		public InfoType Info { get; protected set; }

		public InfoPacket(InfoType infoType) : base(0)
		{
			this.PacketType = PacketType.Info;
			this.Info = infoType;
		}

		public InfoPacket(InfoType infoType, byte[] responseData=null) : base(0)
		{
			this.PacketType = PacketType.Info;
			this.Info = infoType;
			ResponseData = responseData;
		}

		public new byte[] GetBinaryData()
		{
			if(ResponseData != null)
				return Util.AttachByteArray(base.GetBinaryData(), BitConverter.GetBytes((byte)Info), BitConverter.GetBytes(ResponseData.Length), ResponseData);
			else
				return Util.AttachByteArray(base.GetBinaryData(), BitConverter.GetBytes((byte)Info), BitConverter.GetBytes(0));
		}
	}
}
