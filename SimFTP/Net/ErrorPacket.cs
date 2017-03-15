using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimFTP.Net.MetadataPackets;

namespace SimFTP.Net
{
	public enum ErrorType
	{
		Not_Accepted_Packet,
		Not_Accepted_Anonymous,
		Wrong_Certificate,
		Security_Alert
	}

	public class ErrorPacket : InfoPacket
	{
		public ErrorType ErrorType { get; private set; }

		public ErrorPacket(ErrorType error) : base(InfoType.Error)
		{
			ErrorType = error;
		}

		public new byte[] GetBinaryData()
		{
			return Util.AttachByteArray(base.GetBinaryData(), Util.ByteToByteArray((byte)ErrorType));
		}
	}
}
