using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simple_File_Transfer.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_File_Transfer.Net.Tests
{
	[TestClass()]
	public class FileDataFrameTests
	{
		[TestMethod()]
		public void FileDataFrameTest()
		{
			string fn = "test";
			byte[] fd = new byte[8];


			FileDataFrame fdf = new FileDataFrame((ushort)fn.Length, (ulong)fd.LongLength, fn.ToArray(), fd);
		}

		[TestMethod()]
		public void GetBinaryDataTest()
		{
			string fn = "test";
			byte[] fd = new byte[8];


			FileDataFrame fdf = new FileDataFrame((ushort)fn.Length, (ulong)fd.LongLength, fn.ToArray(), fd);
			byte[] fs = fdf.GetBinaryData();
		}
	}
}