using System;
using System.Collections.Generic;
using System.Text;

namespace CPU_Emulator.Core.CPUs._6502.Assembler
{
	internal class ProgramInstruction_6502
	{
		public uint LineNumber { get; init; }
		public string? Label { get; init; }
		public string? Name { get; init; }
		public ParsedOperand_6502? Operand { get; init; }
		public byte Size { get; set; }


		public ProgramInstruction_6502(uint lineNumber, string? label, string? name, ParsedOperand_6502? operand, byte size)
		{
			LineNumber = lineNumber;
			Label = label;
			Name = name;
			Operand = operand;
			Size = size;
		}
	}
}
