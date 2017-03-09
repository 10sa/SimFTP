using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Security.Cryptography;
using SimFTP;

namespace SimFTP.Security
{
	public sealed class AES256Manager
	{
		// Getter, Setter. (For Key)
		public byte[] Key { get { return AESManager.Key; } private set { AESManager.Key = value; } }
		private byte[] IV = new byte[Util.HashByteSize / 2];

		private RijndaelManaged AESManager = new RijndaelManaged();

		public AES256Manager()
		{
			Random keyValue = new Random();

			SetCryptoConfig(Util.GetHashValue(BitConverter.GetBytes(Environment.TickCount * keyValue.Next())));
			SetAESConfigs();
		}

		public AES256Manager(byte[] key)
		{
			SetCryptoConfig(key);
			SetAESConfigs();
		}

		private void SetCryptoConfig (byte[] key)
		{
			Key = key;
			Array.Copy(Key, IV, IV.Length);
		}

		private void SetAESConfigs ()
		{
			AESManager.Padding = PaddingMode.PKCS7;
			AESManager.Mode = CipherMode.CBC;
			AESManager.KeySize = Util.HashByteSize * 8;
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
