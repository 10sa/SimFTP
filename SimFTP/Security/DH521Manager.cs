using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;

namespace SimFTP.Security
{
	public class DH521Manager
	{
		private ECDiffieHellmanCng dh = new ECDiffieHellmanCng(521);
		public byte[] publicKey { get { return dh.PublicKey.ToByteArray(); } }

		public DH521Manager()
		{
			dh.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
			dh.HashAlgorithm = CngAlgorithm.Sha256;
		}

		public byte[] GetShareKey(byte[] key)
		{
			return dh.DeriveKeyMaterial(CngKey.Import(key, CngKeyBlobFormat.EccPublicBlob));
		}
	}
}
