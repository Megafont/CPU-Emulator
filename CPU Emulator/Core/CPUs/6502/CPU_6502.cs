#define ENABLE_DEBUG_CONSOLE_OUTPUT


using CPU_Emulator.Core.Components;
using CPU_Emulator.Core.CPUs._6502.Assembler;
using CPU_Emulator.Core.Interfaces.Components;
using CPU_Emulator.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CPU_Emulator.Core.CPUs._6502
{
	/// <summary>
	/// This class is an implementation of a 6502 processor.
	/// https://6502.org/users/obelisk/programming.html
	/// </summary>
	public partial class CPU_6502
	{
		protected internal IClock _Clock;
		protected internal IRAM_8Bit _RAM;


		// ========================================================================================================================================================================================================
		// REGISTERS
		// ========================================================================================================================================================================================================

		protected internal ushort _REG_ProgramCounter;
		protected internal byte _REG_StackPointer;
		protected internal byte _REG_Accumulator;
		protected internal byte _REG_IndexRegisterX;
		protected internal byte _REG_IndexRegisterY;
		protected internal byte _REG_ProcessorStatus;

		protected internal enum ProcessorStatusBits : byte
		{
			_0_CarryFlag = 0,
			_1_ZeroFlag,
			_2_InteruptDisableFlag,
			_3_DecimalModeFlag,
			_4_UnusedFlag, // Always reads 1.
			_5_BreakCommandFlag,
			_6_OverflowFlag,
			_7_NegativeFlag,

		}


		// ========================================================================================================================================================================================================
		// MEMORY DEFINITIONS
		// ========================================================================================================================================================================================================

		protected const ushort PROGRAM_MEM_START_PAGE = 2;
		protected const UInt16 _RESERVED_BEYOND = 0xFFFA; // The 6502 processor reserves the last 6 bytes of the memory, so everything from 0xFFFA to the end.


		protected enum FixedMemoryPageIndices
		{
			_0_ZeroPage = 0,	// Used for special addressing modes that allow shorter/faster instructions
			_1_SystemStack,		// Holds the system stack
			_2_Program,
		}


		// ========================================================================================================================================================================================================
		// PROPERTIES
		// ========================================================================================================================================================================================================

		public bool AutoStart { get; set; } = false;
		public int DefaultInstructionTickCount { get; set; } = 0;
		public bool IsRunning { get => _Clock.IsRunning; }
		public string Name { get; private set; }
		public ushort ProgramMemoryStartAddress => (ushort) (_RAM.PageSize * PROGRAM_MEM_START_PAGE);

		public bool UseRealisticInstructionDurations { get; protected set; }


		// ========================================================================================================================================================================================================
		// MISC.
		// ========================================================================================================================================================================================================

		protected int _CurrentInstructionTickCount = 0;
		protected bool _InstructionIsRunning;


		/// <summary>
		/// Initializes this CPU.
		/// </summary>
		/// <param name="name">The name of the CPU. Handy for testing/debugging purposes.</param>
		/// <param name="autoStart">If true, the CPU will start running immediately after initialization. Defaults to false.</param>
		/// <param name="clockDuration">
		/// Sets the clock tick speed of the CPU (in ms). Defaults to 1.
		///
		/// WARNING:
		/// Values less than 0.01f may cause the CPU to behave erratically, as in instructions running multiple times.
		/// I think this is because the ticks happen faster than the CPU runs the instruction in such cases.
		/// </param>
		/// <param name="useRealisticInstructionDurations">If true, each instruction will take as many ticks as it would on a real 6502 CPU. Defaults to true.</param>
		/// <param name="defaultInstructionTickCount">The default number of ticks each instruction will take if realistic durations are not used. Defaults to 1.</param>
		public void Initialize(string name, bool autoStart = false, float clockDuration = 1.0f, bool useRealisticInstructionDurations = true, int defaultInstructionTickCount = 1)
		{
			Name = name;
			AutoStart = autoStart;
			DefaultInstructionTickCount = defaultInstructionTickCount;
			Name = name;
			UseRealisticInstructionDurations = useRealisticInstructionDurations;

			InitInstructionSet();

			_Clock = new Clock(clockDuration);
			_Clock.OnTick += OnTick;

			_RAM = new RAM_8Bit();
			_RAM.Initialize(256, 256); // 64KB

			Reset();

			_REG_ProgramCounter = ProgramMemoryStartAddress;

			if (AutoStart)
				Start();
		}

		private void OnTick(object sender, ClockTickEventArgs e)
		{
			if (!IsRunning)
				return;


			if (_CurrentInstructionTickCount > 0)
			{

#if ENABLE_DEBUG_CONSOLE_OUTPUT
				Console.WriteLine($"[CPU_{Name}]: IsRunning={IsRunning}    CurrentTick: {_Clock.CurrentTick}    " +
				                  $"RemainingTicksForCurrentInstruction={_CurrentInstructionTickCount}");
#endif

				_CurrentInstructionTickCount--;
				return;
			}


			// We've consumed as many ticks as the current instruction requires if we made it this far down in this function.
			// So now check if the current instruction is still running. If it is, then we have a problem, as the instruction should have finished running by now.
			if (_InstructionIsRunning)
			{
				Console.WriteLine($"[CPU_{Name}]: ERROR: Previous instruction has not finished running! Next instruction cannot be started on this tick.");
				return;
			}


			_InstructionIsRunning = true;
			ExecuteNextInstruction();

		}

		private void ExecuteNextInstruction()
		{
			// Get the current instruction.
			byte opCode = _RAM[_REG_ProgramCounter];

			// There's no code to run, meaning we've reached the end of the program. Just return and wait for the next tick.
			if (opCode == 0)
			{
				Console.WriteLine($"[CPU_{Name}]: -- PROGRAM ENDED --");

				// Disable the CPU, since it has hit the end of the program.
				Stop();
				return;
			}

			if (!LookUpInstructionByOpcode(opCode, out InstructionDef_6502? instructionDef))
				throw new InvalidOperationException($"Unrecognized opcode encountered: {opCode}");


			ushort endAddress = (ushort)(_REG_ProgramCounter + instructionDef.ExpectedByteCount);
			byte[] instructionByteCode = _RAM[_REG_ProgramCounter..endAddress];

#if ENABLE_DEBUG_CONSOLE_OUTPUT
			Console.WriteLine($"[CPU_{Name}]: BEFORE INSTRUCTION {opCode.ToString("x2").ToUpper()}-{instructionDef.AssemblyCmdName}-{instructionDef.AddressingMode} [{string.Join(" ", instructionByteCode.Select(b => b.ToString("X2"))), -DEBUG_OUTPUT_INSTRUCTION_FIELD_LEN}]:  " +
			                  $"CPU Status: {_REG_ProcessorStatus.ToBitString()}  |  Accum: {_REG_Accumulator}");
#endif


			// If UseRealisticInstructionDurations is true, set the current instruction delay to the execution duration of the instruction.
			// Otherwise, just leave _CurrentInstructionDelay at the default value of 1 tick.
			//
			// NOTE:
			// We subtract 1 to account for the tick that's running right now, and will be done by the time the debug output in OnTick()
			// runs again.
			_CurrentInstructionTickCount = UseRealisticInstructionDurations
				? instructionDef.ExecutionDuration - 1
				: DefaultInstructionTickCount - 1;


			// Execute the instruction.
			instructionDef.ExecutionDelegate(instructionByteCode);

			// Update the program counter register to point to the next instruction.
			_REG_ProgramCounter += (ushort) instructionByteCode.Length;

#if ENABLE_DEBUG_CONSOLE_OUTPUT
			Console.WriteLine($"[CPU_{Name}]:  AFTER INSTRUCTION {opCode.ToString("x2").ToUpper()}-{instructionDef.AssemblyCmdName}-{instructionDef.AddressingMode} [{string.Join(" ", instructionByteCode.Select(b => b.ToString("X2"))),-DEBUG_OUTPUT_INSTRUCTION_FIELD_LEN}]:  " +
			                  $"CPU Status: {_REG_ProcessorStatus.ToBitString()}  |  Accum: {_REG_Accumulator}");
#endif



			_InstructionIsRunning = false;
		}

		public bool WriteInstructionsToMemory(byte[] instructions)
		{
			ushort startAddress = (ushort) (_RAM.PageSize * PROGRAM_MEM_START_PAGE);
			ushort endAddress = (ushort) (startAddress + (uint) instructions.Length);


			if (startAddress < ProgramMemoryStartAddress || endAddress > _RESERVED_BEYOND)
			{
				throw new InvalidOperationException(
					"The passed in byte array cannot be written to memory, as it would overflow into the reserved bytes at the end of RAM!");
			}


			_RAM.WriteBlockAt(instructions, startAddress);

			// Implementation for writing instructions to memory
			return true;
		}

		public void Start()
		{
			_Clock.Start();
		}

		public void Stop()
		{
			_Clock.Stop();
		}

		public void Reset()
		{
			_Clock.Reset();
			_RAM.Reset();

			ResetRegisters();
		}

		private void ResetRegisters()
		{
			_REG_ProgramCounter = 0;
			_REG_StackPointer = (byte) (_RAM.PageSize - 1);
			_REG_Accumulator = 0;
			_REG_IndexRegisterX = 0;
			_REG_IndexRegisterY = 0;
			_REG_ProcessorStatus = 0;


			// Set the unused bit to 1 in the processor status register, as it should always just be 1.
			MemoryUtils.WriteBit(
				ref _REG_ProcessorStatus,
				(byte) ProcessorStatusBits._4_UnusedFlag,
				true);
		}

	}
}
