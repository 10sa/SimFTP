using System;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace Simple_File_Transfer.Net.MetadataPackets
{
	public enum PacketType
	{
		BasicFrame,
		Error,
		Request,
		BasicSecurity,
		ExpertSecurity
	}

	public class BasicMetadataPacket
	{
		public PacketType PacketType { get; protected set; }
		public int DataCount { get; private set; }

		public BasicMetadataPacket(int dataCount)
		{
			this.PacketType = PacketType.BasicFrame;
			this.DataCount = dataCount;

			return;
		}

		public byte[] GetBinaryData()
		{
			return Util.AttachByteArray(Util.ByteToByteArray((byte)PacketType), BitConverter.GetBytes(DataCount));
		}
	}
}