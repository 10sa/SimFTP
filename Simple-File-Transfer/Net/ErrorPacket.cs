using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Simple_File_Transfer.Net.MetadataPackets;

namespace Simple_File_Transfer.Net
{
	public enum ErrorType
	{
		Not_Accepted_Packet,
		Not_Accepted_Anonymous,
		Wrong_Certificate,
		Security_Alert
	}

	public class ErrorPacket : BasicMetadataPacket
	{
		public ErrorType error { get; private set; }

		public ErrorPacket(ErrorType error) : base(0)
		{
			PacketType = PacketType.Error;
			this.error = error;
		}

		public new byte[] GetBinaryData()
		{
			return Util.AttachByteArray(Util.ByteToByteArray((byte)error), base.GetBinaryData());
		}
	}
}
