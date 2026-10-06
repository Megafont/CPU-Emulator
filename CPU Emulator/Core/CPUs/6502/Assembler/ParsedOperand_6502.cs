using System;
using System.Collections.Generic;
using System.Text;

namespace CPU_Emulator.Core.CPUs._6502.Assembler
{
	// This record could also be shortened to:
	//		internal record ParsedOperand_6502(CPU_6502_AddressingModes AddressingMode, string Expression, int Size)
	internal record ParsedOperand_6502 // This is the same as "internal record class ParsedOperand".
	{
		public AddressingModes_6502 AddressingMode;
		public string Expression;
		public byte Size;


		public ParsedOperand_6502(AddressingModes_6502 addressingMode, string expression, byte size)
		{
			AddressingMode = addressingMode;
			Expression = expression;
			Size = size;
		}
	}
}
