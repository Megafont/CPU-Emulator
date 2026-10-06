using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace CPU_Emulator.Core.CPUs._6502.Assembler
{
	/// <summary>
	/// Used to store program data as the assembler parses the source code. It is not used after the program is assembled.
	/// </summary>
	internal sealed class AsmProgram_6502
	{
		public List<ProgramInstruction_6502> Instructions { get; } = new();
		public Dictionary<string, int> Labels { get; } = new();

		public int ProgramCounterStart { get; init; }
	}
}
