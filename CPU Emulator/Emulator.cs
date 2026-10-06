using System;
using System.Collections.Generic;
using System.Text;

using CPU_Emulator.Core.CPUs._6502;
using CPU_Emulator.Core.CPUs._6502.Assembler;
using CPU_Emulator.Core.Utils;


namespace CPU_Emulator
{
	internal class Emulator
	{
		private CPU_6502 _CPU;


		public Emulator()
		{
			_CPU = new();
			_CPU.Initialize("CPU1", clockDuration: 1);

			//byte[] byteCode = Assembler_6502.Assemble(new[] { $"LDA #00" });
			byte[] byteCode = Assembler_6502.Assemble(new[] { $"LDA $00" });

			_CPU.WriteInstructionsToMemory(byteCode);
			_CPU._RAM[0] = 5;
			//_CPU._RAM.DEBUG_PrintPage(0);
			_CPU.Start();
		}

	}
}
