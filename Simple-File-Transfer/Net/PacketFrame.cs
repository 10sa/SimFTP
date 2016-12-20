using System;
using System.Security.Cryptography;
using System.Collections;
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

		// Refactoring Target //
		public byte[] GetBinaryData()
		{
			byte[] binaryFileNameLenght = BitConverter.GetBytes(FileNameLenght);
			byte[] binaryFileSize = BitConverter.GetBytes(FileSize);
			byte[] binaryFileName = Encoding.UTF8.GetBytes(FileName);

			long bufferSize = binaryFileNameLenght.LongLength + binaryFileSize.LongLength + binaryFileName.LongLength + FileData.LongLength;
			
			byte[] buffer = new byte[bufferSize];
			
			long dataLenght = binaryFileNameLenght.LongLength;
			Array.Copy(binaryFileNameLenght, buffer, dataLenght);

			Array.Copy(binaryFileSize, 0, buffer, dataLenght, binaryFileSize.LongLength);
			dataLenght += binaryFileSize.LongLength;

			Array.Copy(binaryFileName, 0, buffer, dataLenght, binaryFileName.LongLength);
			dataLenght += binaryFileName.LongLength;

			Array.Copy(FileData, 0, buffer, dataLenght, FileData.LongLength);

			return buffer;
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