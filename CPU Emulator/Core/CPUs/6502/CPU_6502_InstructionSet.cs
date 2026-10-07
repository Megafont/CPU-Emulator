using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

using CPU_Emulator.Core.Components;
using CPU_Emulator.Core.Interfaces.Components;
using CPU_Emulator.Core.Utils;

namespace CPU_Emulator.Core.CPUs._6502
{
	/// <summary>
	/// This part of the 6502 CPU implementation defines its instruction set.
	/// </summary>
	/// <remarks>
	/// Many of the commands implemented in this file have multiple modes like Immediate, ZeroPage, etc.
	/// You can find out more here: https://6502.org/users/obelisk/programming.html
	/// </remarks>
	public partial class CPU_6502
	{
		protected Dictionary<byte, InstructionDef_6502> _LookUpInstructionByOpcode;
		protected Dictionary<string, InstructionDef_6502> _LookupInstructionByName;


		private void InitInstructionSet()
		{
			// Define the instruction set.
			List<InstructionDef_6502> instructions = new List<InstructionDef_6502>
			{

				new("LDA", 0xA9, 2, 2, AddressingModes_6502.Immediate,         Run_LDA_Immediate),
				new("LDA", 0xA5, 2, 3, AddressingModes_6502.ZeroPage,          Run_LDA_ZeroPage),
				new("LDA", 0xB5, 2, 4, AddressingModes_6502.ZeroPage_X,        Run_LDA_ZeroPage_X),
				new("LDA", 0xAD, 3, 4, AddressingModes_6502.Absolute,          Run_LDA_Absolute),
				new("LDA", 0xBD, 3, 4, AddressingModes_6502.Absolute_X,        Run_LDA_AbsoluteX),
				new("LDA", 0xB9, 3, 4, AddressingModes_6502.Absolute_Y,        Run_LDA_AbsoluteY),
				new("LDA", 0xA1, 2, 6, AddressingModes_6502.IndexedIndirect,   Run_LDA_IndexedIndirect), // Aka Indirect,X.
				new("LDA", 0xB1, 2, 5, AddressingModes_6502.IndirectIndexed,   Run_LDA_IndirectIndexed), // Aka Indirect,Y.

				new("STA", 0x85, 2, 3, AddressingModes_6502.ZeroPage,          Run_STA_ZeroPage),
				new("STA", 0x95, 2, 4, AddressingModes_6502.ZeroPage_X,        Run_STA_ZeroPage_X),
				new("STA", 0x8D, 3, 4, AddressingModes_6502.Absolute,          Run_STA_Absolute),
				new("STA", 0x9D, 3, 5, AddressingModes_6502.Absolute_X,        Run_STA_Absolute_X),
				new("STA", 0x99, 3, 5, AddressingModes_6502.Absolute_Y,        Run_STA_Absolute_Y),
				new("STA", 0x81, 2, 6, AddressingModes_6502.IndexedIndirect,   Run_STA_IndexedIndirect), // Aka Indirect,X.
				new("STA", 0x91, 2, 6, AddressingModes_6502.IndirectIndexed,   Run_STA_IndirectIndexed), // Aka Indirect,Y.
			};

			// Create and populate the lookup tables.
			_LookupInstructionByName = new();
			_LookUpInstructionByOpcode = new();

			foreach (InstructionDef_6502 instruction in instructions)
			{
				_LookupInstructionByName.Add(instruction.NameKey, instruction);
				_LookUpInstructionByOpcode.Add(instruction.OpCode, instruction);
			}
		}

		public bool LookUpInstructionByOpcode(
			byte opcode,
			out InstructionDef_6502? instructionDef)
		{
			instructionDef = null;

			if (!_LookUpInstructionByOpcode.TryGetValue(opcode, out instructionDef))
			{
				return false;
			}


			return true;
		}

		public bool LookUpInstructionByAssemblyNameAndAddressingMode(
			string assemblyInstructionName, 
			AddressingModes_6502 addressingMode,
			out InstructionDef_6502? instructionDef)
		{
			instructionDef = null;

			List<InstructionDef_6502> results = _LookupInstructionByName.Values.Where(x =>
				x.AssemblyCmdName.ToLower() == assemblyInstructionName.ToLower() && 
				x.AddressingMode == addressingMode)
				.ToList();


			if (results.Count > 1)
			{
				throw new Exception(
					$"Multiple instructions were found with assembly instruction name \"{assemblyInstructionName}\" and memory addressing type {addressingMode.ToString()}!" +
					"This suggests there is a duplicate entry in the instruction set lookup tables!");
			}

			if (results.Count > 0)
			{
				instructionDef = results[0];
				return true;
			}

			return false;
		}


		// ========================================================================================================================================================================================================
		// LDA
		// ========================================================================================================================================================================================================

		private void Run_LDA_Immediate(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			_REG_Accumulator = parameters[1];
			UpdateZeroAndNegativeFlagsBasedOnAccumulator();
		}

		private void Run_LDA_ZeroPage(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			byte ramAddress = parameters[1];
			_REG_Accumulator = _RAM[ramAddress];

			UpdateZeroAndNegativeFlagsBasedOnAccumulator();
		}

		private void Run_LDA_ZeroPage_X(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			byte ramAddress = GetMemoryAddress_ZeroPageX(parameters[1]);
			_REG_Accumulator = _RAM[ramAddress];

			UpdateZeroAndNegativeFlagsBasedOnAccumulator();
		}

		private void Run_LDA_Absolute(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			ushort ramAddress = MemoryUtils.ReadUInt16LittleEndian(parameters, 1);
			_REG_Accumulator =_RAM[ramAddress];

			UpdateZeroAndNegativeFlagsBasedOnAccumulator();
		}

		private void Run_LDA_AbsoluteX(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			ushort baseAddress = MemoryUtils.ReadUInt16LittleEndian(parameters, 1);
			ushort absolute_X_Address = GetMemoryAddress_AbsoluteX(baseAddress);
			_REG_Accumulator = _RAM[absolute_X_Address];

			// If a page was crossed, this instruction takes an extra tick to run on a real 6502 CPU.
			if (_RAM.GetPage(baseAddress) != _RAM.GetPage(absolute_X_Address))
				_CurrentInstructionTickCount++;

			UpdateZeroAndNegativeFlagsBasedOnAccumulator();
		}

		private void Run_LDA_AbsoluteY(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			ushort baseAddress = MemoryUtils.ReadUInt16LittleEndian(parameters, 1);
			ushort absolute_Y_Address = GetMemoryAddress_AbsoluteY(baseAddress);
			_REG_Accumulator = _RAM[absolute_Y_Address];

			// If a page was crossed, this instruction takes an extra tick to run on a real 6502 CPU.
			if (_RAM.GetPage(baseAddress) != _RAM.GetPage(absolute_Y_Address))
				_CurrentInstructionTickCount++;

			UpdateZeroAndNegativeFlagsBasedOnAccumulator();
		}

		private void Run_LDA_IndexedIndirect(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			// NOTE: This addressing mode is always 6 ticks on a real 6502, even when the effective zero page
			//		 address wraps around, since the 8-bit pointer math can never cross a page boundary.
			ushort finalAddress = GetMemoryAddress_IndexedIndirect(parameters[1]);

			_REG_Accumulator = _RAM[finalAddress];

			UpdateZeroAndNegativeFlagsBasedOnAccumulator();
		}

		private void Run_LDA_IndirectIndexed(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			ushort finalAddress = GetMemoryAddress_IndirectIndexed(parameters[1], out ushort basePointerAddress);

			// If adding the Y register to the pointer crossed a page boundary, this instruction takes an extra tick to run on a real 6502 CPU.
			if (_RAM.GetPage(basePointerAddress) != _RAM.GetPage(finalAddress))
				_CurrentInstructionTickCount++;

			_REG_Accumulator = _RAM[finalAddress];

			UpdateZeroAndNegativeFlagsBasedOnAccumulator();
		}


		// ========================================================================================================================================================================================================
		// LDA
		// ========================================================================================================================================================================================================

		private void Run_STA_ZeroPage(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			byte ramAddress = parameters[1];
			_RAM[ramAddress] = _REG_Accumulator;
		}

		private void Run_STA_ZeroPage_X(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			// NOTE: The 6502 discards the carry when indexing into the zero page, so this address wraps around
			//		 to the start of the zero page instead of moving up to the next page.
			byte ramAddress = GetMemoryAddress_ZeroPageX(parameters[1]);
			_RAM[ramAddress] = _REG_Accumulator;
		}

		private void Run_STA_Absolute(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			ushort ramAddress = MemoryUtils.ReadUInt16LittleEndian(parameters, 1);
			_RAM[ramAddress] = _REG_Accumulator;
		}

		private void Run_STA_Absolute_X(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			ushort ramAddress = GetMemoryAddress_AbsoluteX(MemoryUtils.ReadUInt16LittleEndian(parameters, 1));
			_RAM[ramAddress] = _REG_Accumulator;
		}

		private void Run_STA_Absolute_Y(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			ushort ramAddress = GetMemoryAddress_AbsoluteY(MemoryUtils.ReadUInt16LittleEndian(parameters, 1));
			_RAM[ramAddress] = _REG_Accumulator;
		}

		private void Run_STA_IndexedIndirect(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			// NOTE: This addressing mode is always 6 ticks on a real 6502, since the 8-bit pointer math can
			//		 never cross a page boundary.
			ushort finalAddress = GetMemoryAddress_IndexedIndirect(parameters[1]);

			_RAM[finalAddress] = _REG_Accumulator;
		}

		private void Run_STA_IndirectIndexed(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			// NOTE: This instruction is always 6 ticks on a real 6502, since indexed writes always pay the
			//		 page-cross penalty no matter what.
			ushort finalAddress = GetMemoryAddress_IndirectIndexed(parameters[1], out _);

			_RAM[finalAddress] = _REG_Accumulator;
		}
	}


}
