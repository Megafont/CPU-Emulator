using System;
using System.Collections.Generic;
using System.Text;

namespace CPU_Emulator.Core.CPUs._6502
{
	/// <summary>
	/// This class holds the definition data for a 6502 instruction.
	/// </summary>
	public class InstructionDef_6502
	{
		private static string[] _AddressingModeCmdSuffixes = new string[]
		{
			"_Imm",		// Immediate
			"_ZP",		// Zero Page
			"_ZPX",		// Zero Page, X
			"_ZPY",		// Zero Page, Y
			"_R",		// Relative
			"_A",		// Absolute
			"_AX",		// Absolute, X
			"_AY",		// Absolute, Y
			"_Idr",		// Indirect
			"_IdxIdr",	// Indexed Indirect
			"_IdrIdx"	// Indirect Indexed

		};

		public static string GetAddressingModeCmdSuffix(int index)
		{
			return _AddressingModeCmdSuffixes[index];
		}



		public delegate void InstructionExecutionDelegate(byte[] parameters);
		public string NameKey { get; private set; } // This is used to look up an instruction in the lookup by name dictionary. It adds a memory addressing mode suffix to differentiate versions of the same instruction.
		public string AssemblyCmdName { get; private set; }
		public AddressingModes_6502 AddressingMode { get; private set; }
		public ushort ExpectedByteCount { get; private set; } // This holds the expected byte count of the instruction in compiled form.
		public byte OpCode { get; private set; }

		// The number of ticks it takes this instruction to run if the CPU's UseRealisticInstructionDurations option is on.
		public byte ExecutionDuration { get; private set; }

		public InstructionExecutionDelegate ExecutionDelegate { get; private set; }


		public InstructionDef_6502(string assemblyCmdName, byte opCode, ushort expectedByteCount, byte executionDuration, AddressingModes_6502 addressingMode, InstructionExecutionDelegate instructionExecutionDelegate)
		{
			AssemblyCmdName = assemblyCmdName;
			OpCode = opCode;
			ExpectedByteCount = expectedByteCount;
			ExecutionDuration = executionDuration;
			AddressingMode = addressingMode;
			ExecutionDelegate = instructionExecutionDelegate;
			NameKey = assemblyCmdName + _AddressingModeCmdSuffixes[(int) addressingMode];
		}
	}
}
