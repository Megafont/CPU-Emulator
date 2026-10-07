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
		// NOTE: This is keyed by the addressing mode enum itself instead of an array indexed by the enum's ordinal,
		//		 so adding or reordering enum members can never silently shift the suffixes (or run off the array end).
		public static string GetAddressingModeCmdSuffix(AddressingModes_6502 addressingMode)
		{
			return addressingMode switch
			{
				AddressingModes_6502.Implied => "_Impl",
				AddressingModes_6502.Accumulator => "_Acc",
				AddressingModes_6502.Immediate => "_Imm",
				AddressingModes_6502.ZeroPage => "_ZP",
				AddressingModes_6502.ZeroPage_X => "_ZPX",
				AddressingModes_6502.ZeroPage_Y => "_ZPY",
				AddressingModes_6502.Relative => "_R",
				AddressingModes_6502.Absolute => "_A",
				AddressingModes_6502.Absolute_X => "_AX",
				AddressingModes_6502.Absolute_Y => "_AY",
				AddressingModes_6502.Indirect => "_Idr",
				AddressingModes_6502.IndexedIndirect => "_IdxIdr",
				AddressingModes_6502.IndirectIndexed => "_IdrIdx",
				_ => throw new ArgumentOutOfRangeException(nameof(addressingMode)),
			};
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
			NameKey = assemblyCmdName + GetAddressingModeCmdSuffix(addressingMode);
		}
	}
}
