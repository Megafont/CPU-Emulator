using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CPU_Emulator_Tests.Core.Utils
{
	internal static class BitManipulationTestsData
	{
		internal static IEnumerable TestCasesForGetBit()
		{
			yield return new TestCaseData(0, 0).Returns(false);
			yield return new TestCaseData(0, 1).Returns(false);
			yield return new TestCaseData(0, 2).Returns(false);
			yield return new TestCaseData(0, 3).Returns(false);
			yield return new TestCaseData(0, 4).Returns(false);
			yield return new TestCaseData(0, 5).Returns(false);
			yield return new TestCaseData(0, 6).Returns(false);
			yield return new TestCaseData(0, 7).Returns(false);

			yield return new TestCaseData(85, 0).Returns(true);
			yield return new TestCaseData(85, 1).Returns(false);
			yield return new TestCaseData(85, 2).Returns(true);
			yield return new TestCaseData(85, 3).Returns(false);
			yield return new TestCaseData(85, 4).Returns(true);
			yield return new TestCaseData(85, 5).Returns(false);
			yield return new TestCaseData(85, 6).Returns(true);
			yield return new TestCaseData(85, 7).Returns(false);

			yield return new TestCaseData(170, 0).Returns(false);
			yield return new TestCaseData(170, 1).Returns(true);
			yield return new TestCaseData(170, 2).Returns(false);
			yield return new TestCaseData(170, 3).Returns(true);
			yield return new TestCaseData(170, 4).Returns(false);
			yield return new TestCaseData(170, 5).Returns(true);
			yield return new TestCaseData(170, 6).Returns(false);
			yield return new TestCaseData(170, 7).Returns(true);

			yield return new TestCaseData(255, 0).Returns(true);
			yield return new TestCaseData(255, 1).Returns(true);
			yield return new TestCaseData(255, 2).Returns(true);
			yield return new TestCaseData(255, 3).Returns(true);
			yield return new TestCaseData(255, 4).Returns(true);
			yield return new TestCaseData(255, 5).Returns(true);
			yield return new TestCaseData(255, 6).Returns(true);
			yield return new TestCaseData(255, 7).Returns(true);
		}

		internal static IEnumerable TestCasesForFlipAllBits()
		{
			yield return new TestCaseData(0).Returns(255);
			yield return new TestCaseData(85).Returns(170);
			yield return new TestCaseData(170).Returns(85);
			yield return new TestCaseData(255).Returns(0);
		}

		internal static IEnumerable TestCasesForGetBitMask()
		{
			yield return new TestCaseData(0).Returns(1);
			yield return new TestCaseData(1).Returns(2);
			yield return new TestCaseData(2).Returns(4);
			yield return new TestCaseData(3).Returns(8);
			yield return new TestCaseData(4).Returns(16);
			yield return new TestCaseData(5).Returns(32);
			yield return new TestCaseData(6).Returns(64);
			yield return new TestCaseData(7).Returns(128);
		}

		internal static IEnumerable TestCasesForGetInvertedBitMask()
		{
			yield return new TestCaseData(0).Returns(254);
			yield return new TestCaseData(1).Returns(253);
			yield return new TestCaseData(2).Returns(251);
			yield return new TestCaseData(3).Returns(247);
			yield return new TestCaseData(4).Returns(239);
			yield return new TestCaseData(5).Returns(223);
			yield return new TestCaseData(6).Returns(191);
			yield return new TestCaseData(7).Returns(127);
		}

		internal static IEnumerable TestCasesForSetBit()
		{
			yield return new TestCaseData(0, 0).Returns(1);
			yield return new TestCaseData(0, 1).Returns(2);
			yield return new TestCaseData(0, 2).Returns(4);
			yield return new TestCaseData(0, 3).Returns(8);
			yield return new TestCaseData(0, 4).Returns(16);
			yield return new TestCaseData(0, 5).Returns(32);
			yield return new TestCaseData(0, 6).Returns(64);
			yield return new TestCaseData(0, 7).Returns(128);

			yield return new TestCaseData(255, 0).Returns(255);
			yield return new TestCaseData(255, 1).Returns(255);
			yield return new TestCaseData(255, 2).Returns(255);
			yield return new TestCaseData(255, 3).Returns(255);
			yield return new TestCaseData(255, 4).Returns(255);
			yield return new TestCaseData(255, 5).Returns(255);
			yield return new TestCaseData(255, 6).Returns(255);
			yield return new TestCaseData(255, 7).Returns(255);
		}

		internal static IEnumerable TestCasesForClearBit()
		{
			yield return new TestCaseData(0, 0).Returns(0);
			yield return new TestCaseData(0, 1).Returns(0);
			yield return new TestCaseData(0, 2).Returns(0);
			yield return new TestCaseData(0, 3).Returns(0);
			yield return new TestCaseData(0, 4).Returns(0);
			yield return new TestCaseData(0, 5).Returns(0);
			yield return new TestCaseData(0, 6).Returns(0);
			yield return new TestCaseData(0, 7).Returns(0);

			yield return new TestCaseData(255, 0).Returns(254);
			yield return new TestCaseData(255, 1).Returns(253);
			yield return new TestCaseData(255, 2).Returns(251);
			yield return new TestCaseData(255, 3).Returns(247);
			yield return new TestCaseData(255, 4).Returns(239);
			yield return new TestCaseData(255, 5).Returns(223);
			yield return new TestCaseData(255, 6).Returns(191);
			yield return new TestCaseData(255, 7).Returns(127);
		}

		internal static IEnumerable TestCasesForFlipBit()
		{
			yield return new TestCaseData(0, 0).Returns(1);
			yield return new TestCaseData(0, 1).Returns(2);
			yield return new TestCaseData(0, 2).Returns(4);
			yield return new TestCaseData(0, 3).Returns(8);
			yield return new TestCaseData(0, 4).Returns(16);
			yield return new TestCaseData(0, 5).Returns(32);
			yield return new TestCaseData(0, 6).Returns(64);
			yield return new TestCaseData(0, 7).Returns(128);

			yield return new TestCaseData(255, 0).Returns(254);
			yield return new TestCaseData(255, 1).Returns(253);
			yield return new TestCaseData(255, 2).Returns(251);
			yield return new TestCaseData(255, 3).Returns(247);
			yield return new TestCaseData(255, 4).Returns(239);
			yield return new TestCaseData(255, 5).Returns(223);
			yield return new TestCaseData(255, 6).Returns(191);
			yield return new TestCaseData(255, 7).Returns(127);
		}
	}
}
