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
	/// Tests the 6502 LDA instruction.
	/// </summary>
	/// <remarks>
	/// NOTE: I edited this project's *.csproj file by adding the InternalsVisibleTo tag to tell it
	///		  to give this project access to the internals of the CPU Emulator project.
	/// </remarks>
	[TestFixture]
	internal class _6502_InstructionTests_LDA
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
			_Cpu.Initialize("Tests_LDA");
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


		#region LDA Tests

		[Test]
		[TestCase(10, false, false)]
		[TestCase(00, true, false)]
		[TestCase(-5, false, true)]
		[TestCase(255, false, true)]
		public async Task Test_LDA_Immediate(short value, bool expectedZeroFlag, bool expectedNegativeFlag)
		{
			// Arrange
			byte[] byteCode = Assembler_6502.Assemble(new[] { $"LDA #{(byte)value}" });
			_Cpu.WriteInstructionsToMemory(byteCode);

			// Act
			_Cpu.Start();
			await WaitUntilCpuIsDone(_Cpu);
			_Cpu._RAM.DEBUG_PrintPage(0);

			// Assert
			Assert.AreEqual((byte)value, _Cpu._REG_Accumulator, $"The accumulator should contain {(byte)value}!");
			Assert.AreEqual(expectedZeroFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._1_ZeroFlag), $"The zero flag should be {(expectedZeroFlag ? "on" : "off")} since the value is {value}!");
			Assert.AreEqual(expectedNegativeFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._7_NegativeFlag), $"The negative flag should be {(expectedNegativeFlag ? "on" : "off")} since the value is {value}!");
		}

		//// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		[Test]
		[TestCase("00", 4, false, false)]
		[TestCase("10", 8, false, false)]
		[TestCase("20", 0, true, false)]
		[TestCase("64", -5, false, true)]
		[TestCase("80", 64, false, false)]
		[TestCase("A4", 128, false, true)]
		[TestCase("FF", 228, false, true)]
		public async Task Test_LDA_ZeroPage(string address, short value, bool expectedZeroFlag, bool expectedNegativeFlag)
		{
			// Arrange
			ushort base10Address = ushort.Parse(address, NumberStyles.HexNumber);
			_Cpu._RAM[base10Address] = (byte)value; // Write the value to the RAM address being used in this test.

			byte[] byteCode = Assembler_6502.Assemble(new[] { $"LDA ${address}" });
			_Cpu.WriteInstructionsToMemory(byteCode);


			// Act
			_Cpu.Start();
			await WaitUntilCpuIsDone(_Cpu);

			// Assert
			Assert.AreEqual((byte)value, _Cpu._REG_Accumulator, $"The accumulator should contain {(byte)value}!");
			Assert.AreEqual(expectedZeroFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._1_ZeroFlag), $"The zero flag should be {(expectedZeroFlag ? "on" : "off")}!");
			// The negative flag should be on in this case, since the value 255 would indeed have the last bit set even though this technically isn't a negative number. If you interpret this byte as a signed byte, then it would be.
			Assert.AreEqual(expectedNegativeFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._7_NegativeFlag), $"The negative flag should be {(expectedNegativeFlag ? "on" : "off")}!");
		}

		//// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		[Test]
		[TestCase("00", 4, 10, false, false)]
		[TestCase("10", 8, 10, false, false)]
		[TestCase("20", 0, 10, true, false)]
		[TestCase("64", -5, 10, false, true)]
		[TestCase("80", 64, 10, false, false)]
		[TestCase("A4", 128, 10, false, true)]
		[TestCase("FF", 228, 10, false, true)]
		public async Task Test_LDA_ZeroPageX(string address, short value, byte indexXRegisterValue, bool expectedZeroFlag, bool expectedNegativeFlag)
		{
			// Arrange
			ushort base10Address = ushort.Parse(address, NumberStyles.HexNumber);
			_Cpu._REG_IndexRegisterX = indexXRegisterValue;
			ushort finalAddress = (ushort)(base10Address + _Cpu._REG_IndexRegisterX);

			byte[] byteCode = Assembler_6502.Assemble(new[] { $"LDA ${address}, X" });
			_Cpu.WriteInstructionsToMemory(byteCode);
			// NOTE: This calculation has to be cast to a byte so it will rap around properly as it would on the 6502.
			_Cpu._RAM[(byte)finalAddress] = (byte)value;
			Console.WriteLine($"ADDRESS: {address}    NEW ADDRESS: {(ushort) finalAddress}    VALUE: {value}    X-REG VALUE: {_Cpu._REG_IndexRegisterX}");

			// Act
			_Cpu.Start();
			await WaitUntilCpuIsDone(_Cpu);

			// Assert
			Assert.AreEqual((byte)value, _Cpu._REG_Accumulator, $"The accumulator should contain {(byte)value}!");
			Assert.AreEqual(expectedZeroFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._1_ZeroFlag), $"The zero flag should be {(expectedZeroFlag ? "on" : "off")}!");
			// The negative flag should be on in this case, since the value 255 would indeed have the last bit set even though this technically isn't a negative number. If you interpret this byte as a signed byte, then it would be.
			Assert.AreEqual(expectedNegativeFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._7_NegativeFlag), $"The negative flag should be {(expectedNegativeFlag ? "on" : "off")}!");
		}

		//// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		// IMPORTANT: You must be careful with the address here. Any address below 0100 will go to the LDA_ZeroPage execution delegate instead of the
		//			  LDA_AbsoluteX one. This is because the assembler takes any address that fits in one byte and promotes it to 0 page since that's
		//			  a zero-page address anyway.
		[Test]
		[TestCase("0000", 4, false, false)]
		[TestCase("0010", 8, false, false)]
		[TestCase("0020", 0, true, false)]
		[TestCase("0064", -5, false, true)]
		[TestCase("0080", 64, false, false)]
		[TestCase("00A4", 128, false, true)]
		[TestCase("00FF", 228, false, true)]
		[TestCase("DDAA", 99, false, false)]
		[TestCase("FFFF", 120, false, false)]
		public async Task Test_LDA_Absolute(string address, short value, bool expectedZeroFlag, bool expectedNegativeFlag)
		{
			// Arrange
			byte[] byteCode = Assembler_6502.Assemble(new[] { $"LDA ${address}" });
			_Cpu.WriteInstructionsToMemory(byteCode);

			ushort base10Address = ushort.Parse(address, NumberStyles.HexNumber);
			_Cpu._RAM[base10Address] = (byte)value;
			Console.WriteLine($"ADDRESS: {address}    BASE_10_ADDRESS: {base10Address}    VALUE: {value}");

			// Act
			_Cpu.Start();
			await WaitUntilCpuIsDone(_Cpu);

			// Assert
			Assert.AreEqual((byte)value, _Cpu._REG_Accumulator, $"The accumulator should contain {(byte)value}!");
			Assert.AreEqual(expectedZeroFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._1_ZeroFlag), $"The zero flag should be {(expectedZeroFlag ? "on" : "off")}!");
			// The negative flag should be on in this case, since the value 255 would indeed have the last bit set even though this technically isn't a negative number. If you interpret this byte as a signed byte, then it would be.
			Assert.AreEqual(expectedNegativeFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._7_NegativeFlag), $"The negative flag should be {(expectedNegativeFlag ? "on" : "off")}!");
		}

		//// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		// IMPORTANT: You must be careful with the address here. Any address below 0100 will go to the LDA_ZeroPage execution delegate instead of the
		//			  LDA_AbsoluteX one. This is because the assembler takes any address that fits in one byte and promotes it to 0 page since that's
		//			  a zero-page address anyway.
		[Test]
		[TestCase("0100", 4, 10,false, false)]
		[TestCase("0110", 8, 10, false, false)]
		[TestCase("0120", 0, 10, true, false)]
		[TestCase("0164", -5, 10, false, true)]
		[TestCase("0180", 64, 10, false, false)]
		[TestCase("01A4", 128, 10, false, true)]
		[TestCase("01FF", 228, 10, false, true)]
		[TestCase("DDAA", 99, 10, false, false)]
		[TestCase("FFFF", 120, 10, false, false)]
		public async Task Test_LDA_AbsoluteX(string baseAddress, short value, byte indexXRegisterValue, bool expectedZeroFlag, bool expectedNegativeFlag)
		{
			// Arrange
			byte[] byteCode = Assembler_6502.Assemble(new[] { $"LDA ${baseAddress},X" });
			_Cpu.WriteInstructionsToMemory(byteCode);

			ushort base10Address = ushort.Parse(baseAddress, NumberStyles.HexNumber);
			ushort finalAddress = (ushort)(base10Address + indexXRegisterValue);
			_Cpu._REG_IndexRegisterX = indexXRegisterValue;
			_Cpu._RAM[(ushort)(finalAddress)] = (byte) value;
			Console.WriteLine($"ADDRESS: {baseAddress}    BASE_10_ADDRESS: {finalAddress}    VALUE: {value}    X-REG VALUE: {_Cpu._REG_IndexRegisterX}");

			// Act
			_Cpu.Start();
			await WaitUntilCpuIsDone(_Cpu);

			// Assert
			Assert.AreEqual((byte)value, _Cpu._REG_Accumulator, $"The accumulator should contain {(byte)value}!");
			Assert.AreEqual(expectedZeroFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._1_ZeroFlag), $"The zero flag should be {(expectedZeroFlag ? "on" : "off")}!");
			// The negative flag should be on in this case, since the value 255 would indeed have the last bit set even though this technically isn't a negative number. If you interpret this byte as a signed byte, then it would be.
			Assert.AreEqual(expectedNegativeFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._7_NegativeFlag), $"The negative flag should be {(expectedNegativeFlag ? "on" : "off")}!");
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
		public async Task Test_LDA_AbsoluteY(string baseAddress, short value, byte indexYRegisterValue, bool expectedZeroFlag, bool expectedNegativeFlag)
		{
			// Arrange
			byte[] byteCode = Assembler_6502.Assemble(new[] { $"LDA ${baseAddress},Y" });
			_Cpu.WriteInstructionsToMemory(byteCode);

			ushort base10Address = ushort.Parse(baseAddress, NumberStyles.HexNumber);
			ushort finalAddress = (ushort)(base10Address + indexYRegisterValue);
			_Cpu._REG_IndexRegisterY = indexYRegisterValue;
			_Cpu._RAM[finalAddress] = (byte)value;
			Console.WriteLine($"ADDRESS: {baseAddress}    BASE_10_ADDRESS: {finalAddress}    VALUE: {value}    X-REG VALUE: {_Cpu._REG_IndexRegisterY}");

			// Act
			_Cpu.Start();
			await WaitUntilCpuIsDone(_Cpu);

			// Assert
			Assert.AreEqual((byte)value, _Cpu._REG_Accumulator, $"The accumulator should contain {(byte)value}!");
			Assert.AreEqual(expectedZeroFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._1_ZeroFlag), $"The zero flag should be {(expectedZeroFlag ? "on" : "off")}!");
			// The negative flag should be on in this case, since the value 255 would indeed have the last bit set even though this technically isn't a negative number. If you interpret this byte as a signed byte, then it would be.
			Assert.AreEqual(expectedNegativeFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._7_NegativeFlag), $"The negative flag should be {(expectedNegativeFlag ? "on" : "off")}!");
		}

		//// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		[Test]
		[TestCase("00", 4, 10, false, false)]
		[TestCase("10", 8, 10, false, false)]
		[TestCase("20", 0, 10, true, false)]
		[TestCase("64", -5, 10, false, true)]
		[TestCase("80", 64, 10, false, false)]
		[TestCase("A4", 128, 10, false, true)]
		[TestCase("FF", 228, 10, false, true)]
		public async Task Test_LDA_IndexedIndirect(string baseAddress, short value, byte indexXRegisterValue, bool expectedZeroFlag, bool expectedNegativeFlag)
		{
			// Arrange
			// IMPORTANT: The 6502 adds the X register to the zero page address first (wrapping around within
			//		 the zero page), then reads the 2-byte address stored there. The pointer stored at
			//		 (baseAddress + X) contains the baseAddress itself, so the value needs to be written at
			//		 the baseAddress for the CPU to load it.
			ushort base10Address = ushort.Parse(baseAddress, NumberStyles.HexNumber);
			_Cpu._REG_IndexRegisterX = indexXRegisterValue;

			byte effectiveZeroPageAddress = (byte) (base10Address + indexXRegisterValue);
			_Cpu._RAM.WriteUShort(effectiveZeroPageAddress, base10Address); // The 2-byte little-endian pointer the CPU will read from (baseAddress + X).
			_Cpu._RAM[base10Address] = (byte) value; // The value the pointer points at.
			Console.WriteLine($"ADDRESS: {baseAddress}    EFFECTIVE_ZP_ADDRESS: {effectiveZeroPageAddress:X2}    VALUE: {value}    X-REG VALUE: {_Cpu._REG_IndexRegisterX}");

			byte[] byteCode = Assembler_6502.Assemble(new[] { $"LDA (${baseAddress},X)" });
			_Cpu.WriteInstructionsToMemory(byteCode);

			// Act
			_Cpu.Start();
			await WaitUntilCpuIsDone(_Cpu);

			// Assert
			Assert.AreEqual((byte)value, _Cpu._REG_Accumulator, $"The accumulator should contain {(byte)value}!");
			Assert.AreEqual(expectedZeroFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._1_ZeroFlag), $"The zero flag should be {(expectedZeroFlag ? "on" : "off")}!");
			// The negative flag should be on in this case, since the value 255 would indeed have the last bit set even though this technically isn't a negative number. If you interpret this byte as a signed byte, then it would be.
			Assert.AreEqual(expectedNegativeFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._7_NegativeFlag), $"The negative flag should be {(expectedNegativeFlag ? "on" : "off")}!");
		}

		//// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		[Test]
		[TestCase("00", 4, 10, false, false)]
		[TestCase("10", 8, 10, false, false)]
		[TestCase("20", 0, 10, true, false)]
		[TestCase("64", -5, 10, false, true)]
		[TestCase("80", 64, 10, false, false)]
		[TestCase("A4", 128, 10, false, true)]
		[TestCase("FF", 228, 10, false, true)]
		public async Task Test_LDA_IndirectIndexed(string baseAddress, short value, byte indexYRegisterValue, bool expectedZeroFlag, bool expectedNegativeFlag)
		{
			// Arrange
			// IMPORTANT: For this addressing mode the 2-byte pointer is read directly from the zero page
			//		 address, and the Y register is added to the pointer itself (not to the zero page
			//		 address). The pointer stored at the baseAddress contains the baseAddress itself, so the
			//		 value needs to be written at (baseAddress + Y) for the CPU to load it.
			ushort base10Address = ushort.Parse(baseAddress, NumberStyles.HexNumber);
			_Cpu._REG_IndexRegisterY = indexYRegisterValue;

			_Cpu._RAM.WriteUShort(base10Address, base10Address); // The 2-byte little-endian pointer the CPU will read from the baseAddress.
			ushort effectiveAddress = (ushort) (base10Address + indexYRegisterValue); // The final address the CPU will load from (pointer + Y).
			_Cpu._RAM[effectiveAddress] = (byte) value; // The value the pointer points at.
			Console.WriteLine($"ADDRESS: {baseAddress}    EFFECTIVE_ADDRESS: {effectiveAddress:X4}    VALUE: {value}    Y-REG VALUE: {_Cpu._REG_IndexRegisterY}");

			byte[] byteCode = Assembler_6502.Assemble(new[] { $"LDA (${baseAddress}),Y" });
			_Cpu.WriteInstructionsToMemory(byteCode);

			// Act
			_Cpu.Start();
			await WaitUntilCpuIsDone(_Cpu);

			// Assert
			Assert.AreEqual((byte)value, _Cpu._REG_Accumulator, $"The accumulator should contain {(byte)value}!");
			Assert.AreEqual(expectedZeroFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._1_ZeroFlag), $"The zero flag should be {(expectedZeroFlag ? "on" : "off")}!");
			// The negative flag should be on in this case, since the value 255 would indeed have the last bit set even though this technically isn't a negative number. If you interpret this byte as a signed byte, then it would be.
			Assert.AreEqual(expectedNegativeFlag, MemoryUtils.GetBit(_Cpu._REG_ProcessorStatus, (byte)CPU_6502.ProcessorStatusBits._7_NegativeFlag), $"The negative flag should be {(expectedNegativeFlag ? "on" : "off")}!");
		}

		#endregion
	}
}
