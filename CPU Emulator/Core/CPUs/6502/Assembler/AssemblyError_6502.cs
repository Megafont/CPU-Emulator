using System;
using System.Collections.Generic;
using System.Text;

namespace CPU_Emulator.Core.CPUs._6502.Assembler
{
	internal sealed class AssemblyException : Exception
	{
		public AssemblyException(string message) 
			: base(message)

		{

		}

		public AssemblyException(string message, long line)
			: base($"Line {line}: {message}")
		{

		}
	}
}
