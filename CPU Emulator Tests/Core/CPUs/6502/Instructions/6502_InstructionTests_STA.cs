using CPU_Emulator.Core.CPUs;
using CPU_Emulator.Core.CPUs._6502;
using CPU_Emulator.Core.CPUs._6502.Assembler;
using CPU_Emulator.Core.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;


namespace CPU_Emulator_Tests.Core.CPUs._6502.Instructions
{
	/// <summary>
	/// Tests the 6502 STA instruction.
	/// </summary>
	/// <remarks>
	/// NOTE: I edited this project's *.csproj file by adding the InternalsVisibleTo tag to tell it
	///		  to give this project access to the internals of the CPU Emulator project.
	/// </remarks>
	[TestFixture]
	internal class _6502_InstructionTests_STA
	{
		private CPU_6502 _Cpu;


		[OneTimeSetUp]
		public void OneTimeSetup()
		{

		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			
		}

		[SetUp]
		public void Setup()
		{
			_Cpu = new CPU_6502();
			_Cpu.Initialize("Tests_STA");
		}

		[TearDown]
		public void TearDown()
		{
			_Cpu = null;
		}


		private async Task WaitUntilCpuIsDone(CPU_6502 cpu)
		{
			while (cpu.IsRunning)
			{
				await Task.Delay(10);
			}
		}


		#region STA Tests

		//// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		[Test]
		[TestCase("00", 4)]
		[TestCase("10", 8)]
		[TestCase("20", 0)]
		[TestCase("64", -5)]
		[TestCase("80", 64)]
		[TestCase("A4", 128)]
		[TestCase("FF", 228)]
		public async Task Test_STA_ZeroPage(string address, short value)
		{
			// Arrange
			ushort base10Address = ushort.Parse(address, NumberStyles.HexNumber);
			_Cpu._REG_Accumulator = (byte)value; // Write the value to the RAM address being used in this test.

			byte[] byteCode = Assembler_6502.Assemble(new[] { $"STA ${address}" });
			_Cpu.WriteInstructionsToMemory(byteCode);


			// Act
			_Cpu.Start();
			await WaitUntilCpuIsDone(_Cpu);

			// Assert
			Assert.AreEqual((byte)value, _Cpu._RAM[base10Address], $"RAM[{base10Address}] should contain {(byte)value}!");
		}

		//// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		[Test]
		[TestCase("00", 4, 10)]
		[TestCase("10", 8, 10)]
		[TestCase("20", 0, 10)]
		[TestCase("64", -5, 10)]
		[TestCase("80", 64, 10)]
		[TestCase("A4", 128, 10)]
		[TestCase("FF", 228, 10)]
		public async Task Test_STA_ZeroPageX(string address, short value, byte indexXRegisterValue)
		{
			// Arrange
			ushort base10Address = ushort.Parse(address, NumberStyles.HexNumber);
			_Cpu._REG_IndexRegisterX = indexXRegisterValue;
			_Cpu._REG_Accumulator = (byte) value;

			byte[] byteCode = Assembler_6502.Assemble(new[] { $"STA ${address}, X" });
			_Cpu.WriteInstructionsToMemory(byteCode);
			// NOTE: This calculation has to be cast to a byte so it will rap around properly as it would on the 6502.
			byte finalAddress = (byte) (base10Address + indexXRegisterValue);
			Console.WriteLine($"ADDRESS: {address}    NEW ADDRESS: {finalAddress:X2}    VALUE: {value}    X-REG VALUE: {_Cpu._REG_IndexRegisterX}");

			// Act
			_Cpu.Start();
			await WaitUntilCpuIsDone(_Cpu);

			// Assert
			Assert.AreEqual((byte)value, _Cpu._RAM[finalAddress], $"RAM[{finalAddress}] should contain {(byte)value}!");
		}

		//// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		// IMPORTANT: You must be careful with the address here. Any address below 0100 will go to the LDA_ZeroPage execution delegate instead of the
		//			  LDA_AbsoluteX one. This is because the assembler takes any address that fits in one byte and promotes it to 0 page since that's
		//			  a zero-page address anyway.
		[Test]
		[TestCase("0000", 4)]
		[TestCase("0010", 8)]
		[TestCase("0020", 0)]
		[TestCase("0064", -5)]
		[TestCase("0080", 64)]
		[TestCase("00A4", 128)]
		[TestCase("00FF", 228)]
		[TestCase("DDAA", 99)]
		[TestCase("FFFF", 120)]
		public async Task Test_STA_Absolute(string address, short value)
		{
			// Arrange
			byte[] byteCode = Assembler_6502.Assemble(new[] { $"STA ${address}" });
			_Cpu.WriteInstructionsToMemory(byteCode);

			ushort base10Address = ushort.Parse(address, NumberStyles.HexNumber);
			_Cpu._REG_Accumulator = (byte)value;
			Console.WriteLine($"ADDRESS: {address}    BASE_10_ADDRESS: {base10Address}    VALUE: {value}");

			// Act
			_Cpu.Start();
			await WaitUntilCpuIsDone(_Cpu);

			// Assert
			Assert.AreEqual((byte)value, _Cpu._RAM[base10Address], $"RAM[{base10Address}] should contain {(byte)value}!");
		}

		//// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		// IMPORTANT: You must be careful with the address here. Any address below 0100 will go to the LDA_ZeroPage execution delegate instead of the
		//			  LDA_AbsoluteX one. This is because the assembler takes any address that fits in one byte and promotes it to 0 page since that's
		//			  a zero-page address anyway.
		[Test]
		[TestCase("0100", 4, 10)]
		[TestCase("0110", 8, 10)]
		[TestCase("0120", 0, 10)]
		[TestCase("0164", -5, 10)]
		[TestCase("0180", 64, 10)]
		[TestCase("01A4", 128, 10)]
		[TestCase("01FF", 228, 10)]
		[TestCase("DDAA", 99, 10)]
		[TestCase("FFFF", 120, 10)]
		public async Task Test_STA_AbsoluteX(string baseAddress, short value, byte indexXRegisterValue)
		{
			// Arrange
			byte[] byteCode = Assembler_6502.Assemble(new[] { $"STA ${baseAddress},X" });
			_Cpu.WriteInstructionsToMemory(byteCode);

			ushort base10Address = ushort.Parse(baseAddress, NumberStyles.HexNumber);
			ushort finalAddress = (ushort) (base10Address + indexXRegisterValue);

			_Cpu._REG_Accumulator = (byte) value;
			_Cpu._REG_IndexRegisterX = indexXRegisterValue;
			Console.WriteLine($"ADDRESS: {baseAddress}    BASE_10_ADDRESS: {finalAddress}    VALUE: {value}    X-REG VALUE: {_Cpu._REG_IndexRegisterX}");

			// Act
			_Cpu.Start();
			await WaitUntilCpuIsDone(_Cpu);

			// Assert
			Assert.AreEqual((byte)value, _Cpu._RAM[finalAddress], $"RAM[{finalAddress}] should contain {(byte)value}!");
		}

		//// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		// IMPORTANT: You must be careful with the address here. Any address below 0100 will go to the LDA_ZeroPage execution delegate instead of the
		//			  LDA_AbsoluteX one. This is because the assembler takes any address that fits in one byte and promotes it to 0 page since that's
		//			  a zero-page address anyway.
		[Test]
		[TestCase("0100", 4, 10, false, false)]
		[TestCase("0110", 8, 10, false, false)]
		[TestCase("0120", 0, 10, true, false)]
		[TestCase("0164", -5, 10, false, true)]
		[TestCase("0180", 64, 10, false, false)]
		[TestCase("01A4", 128, 10, false, true)]
		[TestCase("01FF", 228, 10, false, true)]
		[TestCase("DDAA", 99, 10, false, false)]
		[TestCase("FFFF", 120, 10, false, false)]
		public async Task Test_STA_AbsoluteY(string baseAddress, short value, byte indexYRegisterValue, bool expectedZeroFlag, bool expectedNegativeFlag)
		{
			// Arrange
			byte[] byteCode = Assembler_6502.Assemble(new[] { $"STA ${baseAddress},Y" });
			_Cpu.WriteInstructionsToMemory(byteCode);

			ushort base10Address = ushort.Parse(baseAddress, NumberStyles.HexNumber);
			ushort finalAddress = (ushort)(base10Address + indexYRegisterValue);
			_Cpu._REG_Accumulator = (byte) value;
			_Cpu._REG_IndexRegisterY = indexYRegisterValue;
			Console.WriteLine($"ADDRESS: {baseAddress}    BASE_10_ADDRESS: {finalAddress}    VALUE: {value}    X-REG VALUE: {_Cpu._REG_IndexRegisterY}");

			// Act
			_Cpu.Start();
			await WaitUntilCpuIsDone(_Cpu);

			// Assert
			Assert.AreEqual((byte)value, _Cpu._RAM[finalAddress], $"RAM[{finalAddress}] should contain {(byte)value}!");
		}

		//// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		[Test]
		[TestCase("00", 4, 10)]
		[TestCase("10", 8, 10)]
		[TestCase("20", 0, 10)]
		[TestCase("64", -5, 10)]
		[TestCase("80", 64, 10)]
		[TestCase("A4", 128, 10)]
		[TestCase("FF", 228, 10)]
		public async Task Test_STA_IndexedIndirect(string baseAddress, short value, byte indexXRegisterValue)
		{
			// Arrange
			// IMPORTANT: The 6502 adds the X register to the zero page address first (wrapping around within
			//		 the zero page), then reads the 2-byte address stored there. The pointer stored at
			//		 (baseAddress + X) contains the baseAddress itself, so the CPU will store the accumulator
			//		 at the baseAddress.
			ushort base10Address = ushort.Parse(baseAddress, NumberStyles.HexNumber);
			byte effectiveZeroPageAddress = (byte) (base10Address + indexXRegisterValue);
			_Cpu._REG_Accumulator = (byte) value;
			_Cpu._REG_IndexRegisterX = indexXRegisterValue;
			_Cpu._RAM.WriteUShort(effectiveZeroPageAddress, base10Address); // The 2-byte little-endian pointer the CPU will read from (baseAddress + X).
			Console.WriteLine($"ADDRESS: {baseAddress}    EFFECTIVE_ZP_ADDRESS: {effectiveZeroPageAddress:X2}    VALUE: {value}    X-REG VALUE: {_Cpu._REG_IndexRegisterX}");

			byte[] byteCode = Assembler_6502.Assemble(new[] { $"STA (${baseAddress},X)" });
			_Cpu.WriteInstructionsToMemory(byteCode);

			// Act
			_Cpu.Start();
			await WaitUntilCpuIsDone(_Cpu);

			// Assert
			Assert.AreEqual((byte)value, _Cpu._RAM[base10Address], $"RAM[{base10Address}] should contain {(byte)value}!");
		}

		//// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		[Test]
		[TestCase("00", 4, 10)]
		[TestCase("10", 8, 10)]
		[TestCase("20", 0, 10)]
		[TestCase("64", -5, 10)]
		[TestCase("80", 64, 10)]
		[TestCase("A4", 128, 10)]
		[TestCase("FF", 228, 10)]
		public async Task Test_STA_IndirectIndexed(string baseAddress, short value, byte indexYRegisterValue)
		{
			// Arrange
			// IMPORTANT: For this addressing mode the 2-byte pointer is read directly from the zero page
			//		 address, and the Y register is added to the pointer itself (not to the zero page
			//		 address). The pointer stored at the baseAddress contains the baseAddress itself, so the
			//		 CPU will store the accumulator at (baseAddress + Y).
			ushort base10Address = ushort.Parse(baseAddress, NumberStyles.HexNumber);
			_Cpu._REG_IndexRegisterY = indexYRegisterValue;

			_Cpu._RAM.WriteUShort(base10Address, base10Address); // The 2-byte little-endian pointer the CPU will read from the baseAddress.
			ushort effectiveAddress = (ushort)(base10Address + indexYRegisterValue); // The final address the CPU will store to (pointer + Y).
			_Cpu._REG_Accumulator = (byte)value; // Seed the accumulator with the value the CPU will store.
			Console.WriteLine($"ADDRESS: {baseAddress}    EFFECTIVE_ADDRESS: {effectiveAddress:X4}    VALUE: {value}    Y-REG VALUE: {_Cpu._REG_IndexRegisterY}");

			byte[] byteCode = Assembler_6502.Assemble(new[] { $"STA (${baseAddress}),Y" });
			_Cpu.WriteInstructionsToMemory(byteCode);

			// Act
			_Cpu.Start();
			await WaitUntilCpuIsDone(_Cpu);

			// Assert
			Assert.AreEqual((byte)value, _Cpu._RAM[effectiveAddress], $"RAM[{effectiveAddress}] should contain {(byte)value}!");
		}

		#endregion
	}
}
