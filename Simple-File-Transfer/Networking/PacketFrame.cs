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

	[Serializable]
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

	[Serializable]
	public class FileDataFrame
	{
		public ushort FileNameLenght { get; private set; }
		public ulong FileSize { get; private set; }

		public char[] FileName { get; private set; }
		public byte[] FileData { get; private set; }

		private FileDataFrame() { }

		public FileDataFrame(ushort nameLenght, ulong fileSize, char[] fileName, byte[] fileData)
		{
			this.FileNameLenght = nameLenght;
			this.FileSize = fileSize;
			this.FileName = fileName;
			this.FileData = fileData;

			return;
		}

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

	[Serializable]
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

	[Serializable]
	public sealed class ExpertSecurityPacket : BasicSecurityPacket
	{
		public ECDiffieHellmanCng DH { get; private set; }

		public ExpertSecurityPacket(BasicSecurityPacket baseFrame)
		{

		}
	}
}