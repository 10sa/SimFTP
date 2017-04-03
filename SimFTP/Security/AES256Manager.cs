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
	public sealed class AES256Manager : IDisposable
	{
		// Getter, Setter. (For Key)
		public byte[] Key { get { return AESManager.Key; } private set { AESManager.Key = value; } }
		private byte[] IV = new byte[Util.HashByteSize / 2];

		private RijndaelManaged AESManager = new RijndaelManaged();

		public AES256Manager(byte[] key)
		{
			SetAESConfigs();
			SetCryptoConfig(key);
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

		/*

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

		*/

		public CryptoStream GetEncryptStream(Stream baseStream)
		{
			return new CryptoStream(baseStream, AESManager.CreateEncryptor(Key, IV), CryptoStreamMode.Write);
		}

		public CryptoStream GetDencryptStream(Stream baseStream)
		{
			return new CryptoStream(baseStream, AESManager.CreateDecryptor(Key, IV), CryptoStreamMode.Write);
		}

		#region IDisposable Support
		private bool disposedValue = false; // 중복 호출을 검색하려면

		void Dispose(bool disposing)
		{
			if(!disposedValue)
			{
				if(disposing)
				{
					AESManager.Dispose();
				}

				// TODO: 관리되지 않는 리소스(관리되지 않는 개체)를 해제하고 아래의 종료자를 재정의합니다.
				// TODO: 큰 필드를 null로 설정합니다.

				IV = null;
				disposedValue = true;
			}
		}

		// TODO: 위의 Dispose(bool disposing)에 관리되지 않는 리소스를 해제하는 코드가 포함되어 있는 경우에만 종료자를 재정의합니다.
		// ~AES256Manager() {
		//   // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
		//   Dispose(false);
		// }

		// 삭제 가능한 패턴을 올바르게 구현하기 위해 추가된 코드입니다.
		public void Dispose()
		{
			// 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
			Dispose(true);
			// TODO: 위의 종료자가 재정의된 경우 다음 코드 줄의 주석 처리를 제거합니다.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
