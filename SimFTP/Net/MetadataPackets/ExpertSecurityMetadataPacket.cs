using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SimFTP.Net.MetadataPackets
{
	public sealed class ExpertSecurityMetadataPacket : BasicSecurityMetadataPacket
	{
		public byte[] PublicKey { get; private set; }

		public ExpertSecurityMetadataPacket(BasicSecurityMetadataPacket packet, byte[] publicKey) : base(packet.Username, packet.Password, packet.DataCount)
		{
			PacketType = PacketType.ExpertSecurity;
			PublicKey = publicKey;
		}

		public new byte[] GetBinaryData()
		{
			return Util.AttachByteArray(base.GetBinaryData(), PublicKey);
		}
	}
}
