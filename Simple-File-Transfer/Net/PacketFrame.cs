using System;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Simple_File_Transfer.Net
{
	public enum PacketType
	{
		BasicFrame,
		Error,
		Request,
		BasicSecurity,
		ExpertsSecurity
	}

	public class PacketFrame
	{
		public PacketType PacketType { get; private set; }
		public byte DataCount { get; private set; }

		public PacketFrame(byte dataCount)
		{
			this.PacketType = PacketType.BasicFrame;
			this.DataCount = dataCount;

			return;
		}

		protected PacketFrame(PacketType packetType, byte dataCount)
		{
			this.PacketType = packetType;

			return;
		}

		public byte[] GetBinaryData()
		{
			return Util.AttachByteArray(BitConverter.GetBytes((sbyte)this.PacketType), BitConverter.GetBytes(DataCount));
		}
	}

	public class BasicSecurityPacket : PacketFrame
	{
		public bool IsAnonynomus { get; private set; }
		public string Username { get; private set; }
		public string Password { get; private set; }
		
		public BasicSecurityPacket(byte dataCount) : base(PacketType.BasicSecurity, dataCount)
		{
			SetAnonymous();

			return;
		}

		public BasicSecurityPacket(string username, string password, byte dataCount) : base(PacketType.BasicSecurity, dataCount)
		{
			SetAccount(username, password);

			return;
		}

		private void SetAnonymous()
		{
			this.IsAnonynomus = true;
			this.Username = null;
			this.Password = null;

			return;
		}

		private void SetAccount(string username, string password)
		{
			this.IsAnonynomus = false;
			this.Username = username;
			this.Password = password;

			return;
		}

		public new byte[] GetBinaryData()
		{
			return Util.AttachByteArray(base.GetBinaryData(), BitConverter.GetBytes(IsAnonynomus), GetBytesToInt(Username.Length), GetBytesToInt(Password.Length), UTF8GetBytes(Username), UTF8GetBytes(Password));
		}

		private byte[] GetBytesToInt(int data)
		{
			return BitConverter.GetBytes(data);
		}

		private byte[] UTF8GetBytes(string str)
		{
			return Encoding.UTF8.GetBytes(str);
		}
	}

	public sealed class ExpertSecurityPacket : BasicSecurityPacket
	{
		public ECDiffieHellmanCng DH { get; private set; }

		public ExpertSecurityPacket(BasicSecurityPacket packet) : base(packet.Username, packet.Password, packet.DataCount) { }
	}
}