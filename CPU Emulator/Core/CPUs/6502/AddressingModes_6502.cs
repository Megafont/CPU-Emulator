namespace CPU_Emulator.Core.CPUs._6502;

/// <summary>
/// This enumerates the 6502 processor's memory addressing modes.
/// It is not inside the CPU_6502 class, because InstructionDef_6502.cs needs access to it.
/// </summary>
public enum AddressingModes_6502
{
	Implied = 0, // Implied by the instruction (aka an instruction with no operand).
	Accumulator,
	Immediate,
	
	ZeroPage,
	ZeroPage_X,
	ZeroPage_Y,

	Relative,

	Absolute,
	Absolute_X,
	Absolute_Y,

	Indirect,
	IndexedIndirect, // Also known as Indirect,X
	IndirectIndexed, // Also known as Indirect,Y
}