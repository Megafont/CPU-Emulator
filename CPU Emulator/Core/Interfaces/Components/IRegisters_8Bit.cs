using System;
using System.Collections.Generic;
using System.Text;

namespace CPU_Emulator.Core.Interfaces.Components
{
	public interface IRegisters_8Bit
	{
		public byte Accumulator { get; set; }
		public byte ProgramCounter { get; set; }
		public byte StackPointer { get; set; }


		void Reset();
	}
}
