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
				new("LDA", 0xA9, 2, 2, AddressingModes_6502.Immediate,  Run_LDA_Immediate),
				new("LDA", 0xA5, 2, 3, AddressingModes_6502.ZeroPage,   Run_LDA_ZeroPage),
				new("LDA", 0xB5, 2, 4, AddressingModes_6502.ZeroPage_X, Run_LDA_ZeroPage_X),
				// LDA instruction does not support ZeroPage_Y addressing mode.
				new("LDA", 0xAD, 3, 4, AddressingModes_6502.Absolute,   Run_LDA_Absolute),
				new("LDA", 0xBD, 3, 4, AddressingModes_6502.Absolute_X, Run_LDA_AbsoluteX),
				new("LDA", 0xB9, 3, 4, AddressingModes_6502.Absolute_Y, Run_LDA_AbsoluteY),
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


		public bool InstructionExists(string name)
		{
			foreach (InstructionDef_6502 instructionDef in _LookupInstructionByName.Values)
			{
				if (instructionDef.AssemblyCmdName.ToLower() == name.ToLower())
				{
					return true;
				}
			}
			
			return false;
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

			ushort ramAddress = GetMemoryAddress_AbsoluteX(MemoryUtils.ReadUInt16LittleEndian(parameters, 1));
			_REG_Accumulator = _RAM[ramAddress];

			UpdateZeroAndNegativeFlagsBasedOnAccumulator();
		}

		private void Run_LDA_AbsoluteY(byte[] parameters)
		{
			ValidateParamListLength(parameters);

			ushort ramAddress = GetMemoryAddress_AbsoluteY(MemoryUtils.ReadUInt16LittleEndian(parameters, 1));
			_REG_Accumulator = _RAM[ramAddress];

			UpdateZeroAndNegativeFlagsBasedOnAccumulator();
		}
	}

}
