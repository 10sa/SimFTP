using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Simple_File_Transfer.Net.MetadataPackets
{
	public sealed class ExpertSecurityMetadataPacket : BasicSecurityMetadataPacket
	{
		public ECDiffieHellmanCng DH { get; private set; }

		public ExpertSecurityMetadataPacket(BasicSecurityMetadataPacket packet) : base(packet.Username, packet.Password, packet.DataCount)
		{
			PacketType = PacketType.ExpertSecurity;
		}
	}
}
