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
		BasicSecurity,
		ExpertsSecurity
	}

	public class PacketFrame
	{
		public PacketType PacketType { get; private set; }

		public PacketFrame()
		{
			this.PacketType = PacketType.BasicFrame;

			return;
		}

		protected PacketFrame(PacketType packetType)
		{
			this.PacketType = packetType;

			return;
		}

		public byte[] GetBinaryData()
		{
			return BitConverter.GetBytes((int)this.PacketType);
		}

		protected int GetByteCount(params string[] strings)
		{
			int count = 0;
			foreach(var str in strings)
				count += Encoding.UTF8.GetByteCount(str);

			return count;
		}
	}

	
	public class BasicSecurityPacket : PacketFrame
	{
		public bool IsAnonynomus { get; private set; }
		public string Username { get; private set; }
		public string Password { get; private set; }
		
		public BasicSecurityPacket() : base(PacketType.BasicSecurity)
		{
			SetAnonymous();

			return;
		}

		public BasicSecurityPacket(string username, string password) : base(PacketType.BasicSecurity)
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
			byte[] parentData = base.GetBinaryData();

		}

		protected byte[] ByteArrayAttach(byte[] firstArray, byte[] SecondArray)
		{
			byte[] buffer = new byte[firstArray.LongLength + SecondArray.LongLength];

			Array.Copy(firstArray, buffer, firstArray.LongLength);
			Array.Copy(SecondArray, 0, buffer, firstArray.LongLength, SecondArray.LongLength);

			return buffer;
		}

		private byte[] UTF8GetBytes(string str)
		{
			return Encoding.UTF8.GetBytes(str);
		}

		private int GetDataSize()
		{
			if(this.IsAnonynomus == true)
				return sizeof(bool);
			else
				return (GetByteCount(Username, Password) + sizeof(bool));
		}
	}

	public sealed class ExpertSecurityPacket : BasicSecurityPacket
	{
		public ECDiffieHellmanCng DH { get; private set; }

		public ExpertSecurityPacket(BasicSecurityPacket baseFrame)
		{

		}
	}
}