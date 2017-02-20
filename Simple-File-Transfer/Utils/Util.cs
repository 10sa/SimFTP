using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

using System.Configuration;
using System.Configuration.Assemblies;
using System.Collections.Specialized;


namespace Simple_File_Transfer.Util
{
	public static class Utils
	{
		#region AttachByteArray Main/Sub Methods
		public static byte[] AttachByteArray(params byte[][] arrays)
		{
			byte[] buffer = new byte[GetArraySize(arrays)];

			long bufferPointer = 0;
			foreach (var array in arrays)
			{
				Array.Copy(array, 0, buffer, bufferPointer, array.LongLength);
				bufferPointer += array.LongLength;
			}

			return buffer;
		}

		private static long GetArraySize(params Array[] arrays)
		{
			long size = 0;
			foreach (var array in arrays)
				size += array.LongLength;

			return size;
		}
		#endregion

		public static byte[] GetHashValue(byte[] orignal)
		{
			using (SHA256CryptoServiceProvider csp = new SHA256CryptoServiceProvider())
			{
				return csp.ComputeHash(orignal);
			}
		}

		public static string GetHashedString(string data)
		{
			return Encoding.UTF8.GetString(GetHashValue(Encoding.UTF8.GetBytes(data)));
		}

		public static void WriteFile(byte[] file, string fileName)
		{
			using (FileStream fileStream = new FileStream(fileName, FileMode.CreateNew))
			{
				fileStream.Write(file, 0, file.Length);
				fileStream.FlushAsync();

				return;
			}
		}
	}
}