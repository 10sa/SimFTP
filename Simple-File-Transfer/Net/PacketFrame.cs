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

	public class DataFrame
	{
		public short FileNameLenght { get; private set; }
		public long FileSize { get; private set; }

		public char[] FileName { get; private set; }
		public byte[] FileData { get; private set; }

		private DataFrame() { }

		public DataFrame(short nameLenght, long fileSize, char[] fileName, byte[] fileData)
		{
			this.FileNameLenght = nameLenght;
			this.FileSize = fileSize;
			this.FileName = fileName;
			this.FileData = fileData;

			return;
		}

		public byte[] GetBinaryData()
		{
			byte[][] argArray = { BitConverter.GetBytes(FileNameLenght), BitConverter.GetBytes(FileSize), Encoding.UTF8.GetBytes(FileName, 0, FileName.Length), FileData };
			byte[] buffer = new byte[sizeof(short) + sizeof(long) + FileName.Length + FileData.Length];

			long dataLength = 0;
			foreach(var arg in argArray)
				dataLength = BulidArray(arg, buffer, dataLength);

			return buffer;
		}

		private long BulidArray(byte[] soruceBuffer, byte[] orignalBuffer, long length)
		{
			Array.Copy(soruceBuffer, 0, orignalBuffer, length, soruceBuffer.LongLength);
			return length + soruceBuffer.LongLength;
		}
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
	}

	
	public class BasicSecurityPacket : PacketFrame
	{
		public bool IsAnonynomus { get; private set; }
		public char[] Username { get; private set; }
		public char[] Password { get; private set; }
		
		public BasicSecurityPacket() : base(PacketType.BasicSecurity)
		{
			SetAnonymous();
			return;
		}

		public BasicSecurityPacket(char[] username, char[] password) : base(PacketType.BasicSecurity)
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

		private void SetAccount(char[] username, char[] password)
		{
			this.IsAnonynomus = false;
			this.Username = username;
			this.Password = password;
			return;
		}

		public new byte[] GetBinaryData()
		{
			byte[] parentData = base.GetBinaryData();
			byte[] buffer = new byte[GetDataSize() + parentData.Length];

			byte[] binaryUsername = Encoding.UTF8.GetBytes(Username);

			Array.Copy(parentData, buffer, parentData.Length);
			Array.Copy(binaryUsername, 0, buffer, buffer.Length, binaryUsername.Length);

			return buffer;
		}

		private int GetDataSize()
		{
			if(this.IsAnonynomus == true)
				return sizeof(bool);
			else
				return ((sizeof(char) * (this.Username.Length + this.Password.Length)) + sizeof(bool));
		}
	}

	public class BasicSecurityDataFrame : DataFrame
	{
		public byte[] Checksum { get; private set; }

		public BasicSecurityDataFrame(short nameLenght, long fileSize, char[] fileName, byte[] fileData) : base(nameLenght, fileSize, fileName, fileData)
		{
			GetChecksum(fileData);
			return;
		}

		private void GetChecksum(byte[] fileData)
		{
			using (SHA256CryptoServiceProvider hash = new SHA256CryptoServiceProvider())
			{
				Checksum = hash.ComputeHash(fileData);
			}
		}

		public new byte[] GetBinaryData()
		{
			byte[] parentData = base.GetBinaryData();
			byte[] buffer = new byte[parentData.LongLength + Checksum.LongLength];

			Array.Copy(parentData, buffer, parentData.LongLength);
			Array.Copy(Checksum, 0, buffer, parentData.LongLength, Checksum.LongLength);

			return buffer;
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