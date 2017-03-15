using System;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace SimFTP.Net.MetadataPackets
{
	public enum PacketType
	{
		BasicFrame,
		Error,
		Info,
		BasicSecurity,
		ExpertSecurity
	}

	public class BasicMetadataPacket
	{
		public PacketType PacketType { get; protected set; }
		public int DataCount { get; private set; }

		public BasicMetadataPacket(int dataCount)
		{
			PacketType = PacketType.BasicFrame;
			DataCount = dataCount;

			return;
		}

		public BasicMetadataPacket(int dataCount, PacketType overrideType)
		{
			DataCount = dataCount;
			PacketType = overrideType;
		}

		public byte[] GetBinaryData()
		{
			return Util.AttachByteArray(Util.ByteToByteArray((byte)PacketType), BitConverter.GetBytes(DataCount));
		}
	}
}