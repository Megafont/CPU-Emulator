using System;
using System.Collections.Generic;
using System.Text;

using CPU_Emulator.Core.Utils;


namespace CPU_Emulator_Tests.Core.Utils
{
	[TestFixture]
	public class BitManipulationTests
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
		[TestCaseSource(typeof(BitManipulationTestsData), nameof(BitManipulationTestsData.TestCasesForGetBit))]
		public bool TestGetBit(byte value, byte index)
		{
			// Arrange

			// Act
			bool result = MemoryUtils.GetBit(value, index);

			// Assert
			// NOTE: We have to return the result here rather than using Assert.AreEqual(), because we're using an expected result value in these tests.
			return result;
		}

		[Test]
		[TestCaseSource(typeof(BitManipulationTestsData), nameof(BitManipulationTestsData.TestCasesForSetBit))]
		public byte TestSetBit(byte value, byte index)
		{
			// Arrange

			// Act
			MemoryUtils.SetBit(ref value, index);

			// Assert
			// NOTE: We have to return the result here rather than using Assert.AreEqual(), because we're using an expected result value in these tests.
			return value;
		}

		[Test]
		[TestCaseSource(typeof(BitManipulationTestsData), nameof(BitManipulationTestsData.TestCasesForClearBit))]
		public byte TestClearBit(byte value, byte index)
		{
			// Arrange

			// Act
			MemoryUtils.ClearBit(ref value, index);

			// Assert
			// NOTE: We have to return the result here rather than using Assert.AreEqual(), because we're using an expected result value in these tests.
			return value;
		}

		[Test]
		[TestCaseSource(typeof(BitManipulationTestsData), nameof(BitManipulationTestsData.TestCasesForFlipBit))]
		public byte TestFlipBit(byte value, byte index)
		{
			// Arrange

			// Act
			MemoryUtils.FlipBit(ref value, index);

			// Assert
			// NOTE: We have to return the result here rather than using Assert.AreEqual(), because we're using an expected result value in these tests.
			return value;
		}

		[Test]
		[TestCaseSource(typeof(BitManipulationTestsData), nameof(BitManipulationTestsData.TestCasesForFlipAllBits))]
		public byte TestFlipAllBits(byte value)
		{
			// Arrange

			// Act
			MemoryUtils.FlipAllBits(ref value);

			// Assert
			// NOTE: We have to return the result here rather than using Assert.AreEqual(), because we're using an expected result value in these tests.
			return value;
		}

		[Test]
		[TestCaseSource(typeof(BitManipulationTestsData), nameof(BitManipulationTestsData.TestCasesForGetBitMask))]
		public byte TestGetBitMask(byte index)
		{
			// Arrange

			// Act
			byte result = MemoryUtils.GetBitMask(index);

			// Assert
			// NOTE: We have to return the result here rather than using Assert.AreEqual(), because we're using an expected result value in these tests.
			return result;
		}


		[Test]
		[TestCaseSource(typeof(BitManipulationTestsData), nameof(BitManipulationTestsData.TestCasesForGetInvertedBitMask))]
		public byte TestGetInvertedBitMask(byte index)
		{
			// Arrange

			// Act
			byte result = MemoryUtils.GetInvertedBitMask(index);

			// Assert
			// NOTE: We have to return the result here rather than using Assert.AreEqual(), because we're using an expected result value in these tests.
			return result;
		}
	}
}
