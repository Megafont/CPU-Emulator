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
		/// <param name="baseRamAddress">The base address to read a byte from in RAM.</param>
		private byte GetMemoryAddress_ZeroPageX(byte baseRamAddress)
		{
			// NOTE: This addition can potentially cause the value to wrap around to 0, but this is correct behavior for the 6502 CPU, so we don't need to do any special handling here.
			return (byte) (baseRamAddress + _REG_IndexRegisterX);
		}

		/// <summary>
		/// Adds the value of the IndexY register to the specified memory address.
		/// </summary>
		/// <param name="baseRamAddress">The base address to read a byte from in RAM.</param>
		private byte GetMemoryAddress_ZeroPageY(byte baseRamAddress)
		{
			// NOTE: This addition can potentially cause the value to wrap around to 0, but this is correct behavior for the 6502 CPU, so we don't need to do any special handling here.
			return (byte) (baseRamAddress + _REG_IndexRegisterY);
		}

		// NOTE: We don't have a helper method for absolute addressing, since the 2-byte address doesn't need any modification.

		/// <summary>
		/// Adds the value of the IndexX register to the specified absolute 2-byte memory address.
		/// </summary>
		/// <param name="sourceRamAddress">The base address to read a byte from in RAM.</param>
		private ushort GetMemoryAddress_AbsoluteX(ushort sourceRamAddress)
		{
			// NOTE: This addition can potentially cause the value to wrap around to 0, but this is correct behavior for the 6502 CPU, so we don't need to do any special handling here.
			return (ushort) (sourceRamAddress + _REG_IndexRegisterX);
		}

		/// <summary>
		/// Adds the value of the IndexY register to the specified absolute 2-byte memory address.
		/// </summary>
		/// <param name="sourceRamAddress">The base address to read a byte from in RAM.</param>
		private ushort GetMemoryAddress_AbsoluteY(ushort sourceRamAddress)
		{
			// NOTE: This addition can potentially cause the value to wrap around to 0, but this is correct behavior for the 6502 CPU, so we don't need to do any special handling here.
			return (ushort)(sourceRamAddress + _REG_IndexRegisterY);
		}

		/// <summary>
		/// Retrieves the memory address stored at the specified absolute 2-byte memory address.
		/// </summary>
		/// <param name="sourceRamAddress">The source ram address to read another address from in RAM.</param>
		private ushort GetMemoryAddress_Indirect(ushort sourceRamAddress)
		{
			return _RAM.ReadUShort(sourceRamAddress);
		}

		/// <summary>
		/// Adds the value of the IndexY register to the specified absolute 2-byte memory address.
		/// </summary>
		/// <param name="sourceRamAddress">The base address to read a byte from in RAM.</param>
		private ushort MemoryAccess_IndexedIndirect(ushort sourceRamAddress)
		{
			// NOTE: This addition can potentially cause the value to wrap around to 0, but this is correct behavior for the 6502 CPU, so we don't need to do any special handling here.
			return _RAM[(ushort)(sourceRamAddress + _REG_IndexRegisterX)];
		}
	}

}
