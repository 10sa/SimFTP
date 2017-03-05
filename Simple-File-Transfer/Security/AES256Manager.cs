using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Security.Cryptography;
using Simple_File_Transfer;

namespace Simple_File_Transfer.Security
{
	public sealed class AES256Manager
	{
		private byte[] Key;
		private byte[] IV = new byte[Util.HashSize / 2];

		private RijndaelManaged AESManager = new RijndaelManaged();

		public AES256Manager()
		{
			Random keyValue = new Random();

			Key = Util.GetHashValue(BitConverter.GetBytes(Environment.TickCount * keyValue.Next()));
			Array.Copy(Key, IV, IV.Length);

			AESManager.Padding = PaddingMode.ANSIX923;
			AESManager.Mode = CipherMode.CBC;
			AESManager.KeySize = Util.HashSize * 8;
		}

		public byte[] Encrypt(byte[] data)
		{
			using (MemoryStream encryptMemoryStream = new MemoryStream())
			{
				using (CryptoStream encryptCryptoStream = new CryptoStream(encryptMemoryStream, AESManager.CreateEncryptor(Key, IV), CryptoStreamMode.Write))
				{
					encryptCryptoStream.Write(data, 0, data.Length);
					encryptCryptoStream.FlushFinalBlock();

					return encryptMemoryStream.ToArray();
				}
			}
		}

		public byte[] Decrypt(byte[] data)
		{
			using (MemoryStream decryptMemoryStream = new MemoryStream(data))
			{
				using (CryptoStream decryptCryptoStream = new CryptoStream(decryptMemoryStream, AESManager.CreateDecryptor(Key, IV), CryptoStreamMode.Read))
				{
					byte[] buffer = new byte[data.Length];
					decryptCryptoStream.Read(buffer, 0, buffer.Length);

					return buffer;
				}
			}
		}
	}
}
