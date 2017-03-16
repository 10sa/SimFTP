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

		public ExpertSecurityMetadataPacket(string username, string password, int dataCount, byte[] publicKey) : base(username, password, dataCount)
		{
			PacketType = PacketType.ExpertSecurity;
			PublicKey = publicKey;
		}

		public ExpertSecurityMetadataPacket(int dataCount, byte[] publicKey) : base(dataCount)
		{
			PacketType = PacketType.ExpertSecurity;
			PublicKey = publicKey;
		}

		public ExpertSecurityMetadataPacket(int dataCount, string username, string password, byte[] publicKey) : base(username, password, dataCount)
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
