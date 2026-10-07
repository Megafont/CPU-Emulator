using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using CPU_Emulator.Core.Components;
using CPU_Emulator.Core.Interfaces.Components;
using CPU_Emulator.Core.Utils;

namespace CPU_Emulator.Core.CPUs._6502
{
	/// <summary>
	/// This part of the 6502 CPU implementation defines some helper methods.
	/// </summary>
	public partial class CPU_6502
	{
		private const int DEBUG_OUTPUT_INSTRUCTION_FIELD_LEN = 12;


		// TODO: Change this file to just be Utils_6502, and make it a separate static class if possible?


		/// <summary>
		/// Validates the number of parameters being passed into the specified compiled 6502 instruction.
		/// </summary>
		/// <param name="byteCode">The bytecode of the instruction to check.</param>
		private bool ValidateParamListLength(byte[] byteCode)
		{
			InstructionDef_6502 instructionDef = _LookUpInstructionByOpcode[byteCode[0]];

			if (byteCode.Length != instructionDef.ExpectedByteCount)
			{
				throw new ArgumentException(
					$"Wrong number of parameters! Expected {instructionDef.ExpectedByteCount} but got {byteCode.Length}.");
				
				return false;
			}


			return true;
		}


		// ========================================================================================================================================================================================================
		// COMMON TASK HELPER METHODS
		// ========================================================================================================================================================================================================

		private void UpdateZeroAndNegativeFlagsBasedOnAccumulator()
		{
			MemoryUtils.WriteBit(
				ref _REG_ProcessorStatus,
				(byte)ProcessorStatusBits._1_ZeroFlag,
				_REG_Accumulator == 0);

			MemoryUtils.WriteBit(
				ref _REG_ProcessorStatus,
				(byte)ProcessorStatusBits._7_NegativeFlag,
				MemoryUtils.GetBit(_REG_Accumulator, 7));
		}


		// ========================================================================================================================================================================================================
		// MEMORY ADDRESSING MODES HELPER METHODS
		// ========================================================================================================================================================================================================

		// NOTE: We don't have a helper method for zero-page addressing, since the 1-byte address doesn't need any modification.

		/// <summary>
		/// Adds the value of the IndexX register to the specified memory address.
		/// </summary>
		/// <param name="baseRamAddress">The base address in RAM.</param>
		internal byte GetMemoryAddress_ZeroPageX(byte baseRamAddress)
		{
			// NOTE: This addition can potentially cause the value to wrap around to 0, but this is correct behavior for the 6502 CPU, so we don't need to do any special handling here.
			return (byte) (baseRamAddress + _REG_IndexRegisterX);
		}

		/// <summary>
		/// Adds the value of the IndexY register to the specified memory address.
		/// </summary>
		/// <param name="baseRamAddress">The base address in RAM.</param>
		internal byte GetMemoryAddress_ZeroPageY(byte baseRamAddress)
		{
			// NOTE: This addition can potentially cause the value to wrap around to 0, but this is correct behavior for the 6502 CPU, so we don't need to do any special handling here.
			return (byte) (baseRamAddress + _REG_IndexRegisterY);
		}

		// NOTE: We don't have a helper method for absolute addressing, since the 2-byte address doesn't need any modification.

		/// <summary>
		/// Adds the value of the IndexX register to the specified absolute 2-byte memory address.
		/// </summary>
		/// <param name="sourceRamAddress">The base address in RAM.</param>
		internal ushort GetMemoryAddress_AbsoluteX(ushort sourceRamAddress)
		{
			// NOTE: This addition can potentially cause the value to wrap around to 0, but this is correct behavior for the 6502 CPU, so we don't need to do any special handling here.
			return (ushort) (sourceRamAddress + _REG_IndexRegisterX);
		}

		/// <summary>
		/// Adds the value of the IndexY register to the specified absolute 2-byte memory address.
		/// </summary>
		/// <param name="sourceRamAddress">The base address to in RAM.</param>
		internal ushort GetMemoryAddress_AbsoluteY(ushort sourceRamAddress)
		{
			// NOTE: This addition can potentially cause the value to wrap around to 0, but this is correct behavior for the 6502 CPU, so we don't need to do any special handling here.
			return (ushort)(sourceRamAddress + _REG_IndexRegisterY);
		}

		/// <summary>
		/// Retrieves the memory address stored at the specified absolute 2-byte memory address.
		/// </summary>
		/// <param name="sourceRamAddress">The RAM address to read another address from.</param>
		internal ushort GetMemoryAddress_Indirect(ushort sourceRamAddress)
		{
			return _RAM.ReadUShort(sourceRamAddress);
		}

		/// <summary>
		/// Adds the value of the IndexX register to the specified zero page address, then reads the 2-byte
		/// memory address stored there. This addressing mode is typically used for referencing a table in
		/// the zero page (first page of RAM).
		/// </summary>
		/// <param name="sourceRamAddress">The zero page address holding the 2-byte pointer.</param>
		internal ushort GetMemoryAddress_IndexedIndirect(byte sourceRamAddress)
		{
			// NOTE: The X register is added to the zero page address itself, wrapping around within the zero page.
			byte effectiveZeroPageAddress = GetMemoryAddress_ZeroPageX(sourceRamAddress);

			return ReadZeroPagePointer(effectiveZeroPageAddress);
		}

		/// <summary>
		/// Reads the 2-byte memory address stored at the specified zero page address, then adds the value
		/// of the IndexY register to it. This addressing mode is typically used for referencing a table in
		/// the zero page (first page of RAM).
		/// </summary>
		/// <param name="sourceRamAddress">The zero page address holding the 2-byte pointer.</param>
		/// <param name="basePointerAddress">The 2-byte pointer read from the zero page, before the Y register is added to it.</param>
		internal ushort GetMemoryAddress_IndirectIndexed(byte sourceRamAddress, out ushort basePointerAddress)
		{
			basePointerAddress = ReadZeroPagePointer(sourceRamAddress);

			// NOTE: The Y register is added to the 2-byte pointer itself, so this wraps around at the end of
			//		 all of RAM instead of staying within the zero page.
			return (ushort) (basePointerAddress + _REG_IndexRegisterY);
		}

		/// <summary>
		/// Reads a 2-byte memory address stored in the zero page (first page of RAM).
		/// The 6502 is little-endian, so the low byte of the address is stored first.
		/// </summary>
		/// <param name="zeroPageAddress">The zero page address the 2-byte pointer is stored at.</param>
		internal ushort ReadZeroPagePointer(byte zeroPageAddress)
		{
			// NOTE: Both bytes are read with byte wrapping, so a pointer stored at $FF has its high byte read
			//		 from $00 rather than $0100. This is correct behavior for the 6502 CPU.
			ushort pointerLowByte = _RAM[zeroPageAddress];
			ushort pointerHighByte = _RAM[(byte) (zeroPageAddress + 1)];

			return (ushort) ((pointerHighByte << 8) | pointerLowByte);
		}
	}

}
