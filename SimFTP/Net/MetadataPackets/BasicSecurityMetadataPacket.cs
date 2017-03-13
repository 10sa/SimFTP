using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SimFTP.Net.MetadataPackets
{
	public class BasicSecurityMetadataPacket : BasicMetadataPacket
	{
		public bool IsAnonynomus { get; private set; }
		public string Username { get; private set; }
		public string Password { get; private set; }

		private Encoding UTF8 { get { return Encoding.UTF8; } }

		public BasicSecurityMetadataPacket(int dataCount) : base(dataCount)
		{
			PacketType = PacketType.BasicSecurity;
			SetAnonymous();

			return;
		}

		public BasicSecurityMetadataPacket(string username, string password, int dataCount) : base(dataCount)
		{
			PacketType = PacketType.BasicSecurity;
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
			if (IsAnonynomus)
			{
				byte[] isAnonymous = BitConverter.GetBytes(IsAnonynomus);

				return Util.AttachByteArray(base.GetBinaryData(), isAnonymous);
			}
			else
			{
				byte[] usernameLenght = BitConverter.GetBytes(UTF8.GetByteCount(Username));
				byte[] passwordLenght = BitConverter.GetBytes(UTF8.GetByteCount(Password));
				byte[] username = UTF8.GetBytes(Username);
				byte[] password = UTF8.GetBytes(Password);

				return Util.AttachByteArray(base.GetBinaryData(), BitConverter.GetBytes(false), usernameLenght, passwordLenght, username, password);
			}
		}
	}
}
