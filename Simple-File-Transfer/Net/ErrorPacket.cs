using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_File_Transfer.Net
{
	public enum ErrorType
	{
		Not_Accepted_Default_Packet,
		Wrong_Certificate,
		Security_Alert
	}

	public class ErrorPacket : PacketFrame
	{
		public ErrorType error { get; private set; }

		public ErrorPacket(ErrorType error) : base(PacketType.Error)
		{
			this.error = error;
		}

		public new byte[] GetBinaryData()
		{
			return Util.AttachByteArray(BitConverter.GetBytes((sbyte)error), base.GetBinaryData());
		}
	}
}
