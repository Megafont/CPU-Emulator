using System;
using System.Collections.Generic;
using System.Text;

using CPU_Emulator.Core.Utils;


namespace CPU_Emulator_Tests.Core.Utils
{
	[TestFixture]
	public class MemoryConversionTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[TearDown]
		public void TearDown()
		{

		}


		[Test]
		public void TestReadInt16_BigEndian()
		{
			// Arrange
			byte[] bytes = { 0xF0, 0x02, 0x03, 0x04 };

			// Act
			short s1 = MemoryUtils.ReadInt16BigEndian(bytes, 0);
			short s2 = MemoryUtils.ReadInt16BigEndian(bytes, 1);
			short s3 = MemoryUtils.ReadInt16BigEndian(bytes, 2);

			// Assert
			Assert.AreEqual(-4094, s1, "Reading the first two bytes as a short should return 242!");
			Assert.AreEqual(515, s2, "Reading the 2nd and 3rd bytes as a short should return 515!");
			Assert.AreEqual(772, s3, "Reading the 3rd and 4th bytes as a short should return 772!");
		}

		[Test]
		public void TestReadUInt16_BigEndian()
		{
			// Arrange
			byte[] bytes = { 0xF0, 0x02, 0x03, 0x04 };

			// Act
			ushort s1 = MemoryUtils.ReadUInt16BigEndian(bytes, 0);
			ushort s2 = MemoryUtils.ReadUInt16BigEndian(bytes, 1);
			ushort s3 = MemoryUtils.ReadUInt16BigEndian(bytes, 2);

			// Assert
			Assert.AreEqual(61442, s1, "Reading the first two bytes as a short should return 61442!");
			Assert.AreEqual(515, s2, "Reading the 2nd and 3rd bytes as a short should return 515!");
			Assert.AreEqual(772, s3, "Reading the 3rd and 4th bytes as a short should return 772!");
		}

		[Test]
		public void TestReadUInt16_LittleEndian()
		{
			// Arrange
			byte[] bytes = { 0xF0, 0x02, 0x03, 0x04 };

			// Act
			ushort s1 = MemoryUtils.ReadUInt16LittleEndian(bytes, 0);
			ushort s2 = MemoryUtils.ReadUInt16LittleEndian(bytes, 1);
			ushort s3 = MemoryUtils.ReadUInt16LittleEndian(bytes, 2);

			// Assert
			Assert.AreEqual(752, s1, "Reading the first two bytes little-endian should return 752 (0x02F0)!");
			Assert.AreEqual(770, s2, "Reading the 2nd and 3rd bytes little-endian should return 770 (0x0302)!");
			Assert.AreEqual(1027, s3, "Reading the 3rd and 4th bytes little-endian should return 1027 (0x0403)!");
		}

		[Test]
		public void TestWriteUInt16_LittleEndian()
		{
			// Arrange
			byte[] bytes = new byte[4];

			// Act
			MemoryUtils.WriteUInt16LittleEndian(bytes, 1, 0xDDAA);

			// Assert
			Assert.AreEqual(0, bytes[0], "The byte before the write position should be untouched!");
			Assert.AreEqual(0xAA, bytes[1], "The low byte should be written first!");
			Assert.AreEqual(0xDD, bytes[2], "The high byte should be written second!");
			Assert.AreEqual(0, bytes[3], "The byte after the write position should be untouched!");
		}

	}
}
