using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;

namespace CPU_Emulator.Core.CPUs._6502.Assembler
{
	public static class Assembler_6502
	{
		// TODO: Add support for directives like .org, which sets the initial program counter value (the address where program memory begins). Others include .byte, .word, .fill, etc.

		/// <summary>
		/// This specifies where in memory the program code should start.
		/// </summary>
		/// <remarks>>
		/// According to claude code, this is where the default value 0x8000 came from:
		///
		/// "There's no technical requirement behind it — it's a convention, and honestly a somewhat
		/// opinionated one. Here's the reasoning.
		///
		/// The 6502's 16-bit address space is 64KB, and the low part of it is spoken for: zero page
		/// at $0000–$00FF (too precious to place code — you want it for zero-page operands), the
		/// hardware stack at $0100–$01FF, and on most real machines the bottom several KB is OS or
		/// system workspace. So user code traditionally goes in the upper half, and $8000 is a round
		/// 'start of the upper 32KB' value. On the NES specifically — which is what a lot of hobby
		/// 6502 projects target — cartridge PRG ROM is mapped at exactly $8000–$FFFF, so $8000 is
		/// the conventional start address there (with the reset vector at $FFFC pointing back at
		/// wherever your entry code actually lives).
		///
		/// Different systems use different values, which is why the choice is arbitrary: the C64
		/// typically loads machine code around $0801 or $C000, the Apple II around $0803, the
		/// Atari 2600 starts execution at the top of the cartridge around $F000. For a bare 6502
		/// with no system software, any address works — you just have to set the reset vector to
		/// match."
		/// </remarks>
		//public const uint PROGRAM_COUNTER_START = 0x8000;
		private static uint _ProgramCounterStart;


		delegate byte[] CompileInstructionDelegate(List<string> tokens, CPU_6502 cpu);

		private static CPU_6502 _CPU;


		// REGULAR EXPRESSIONS
		// ======================================================================================================================================================
		
		private static readonly Regex _LabelRegex = new(@"^([A-Za-z_][A-Za-z0-9_]*):", RegexOptions.Compiled);
		private static readonly Regex _CommentRegex = new(@";.*$", RegexOptions.Compiled);
		private static readonly Regex _InstructionRegex = new(@"^([A-Za-z]{2,3})\s*(.*)$", RegexOptions.Compiled);


		// HASH SETS
		// ======================================================================================================================================================

		private static readonly HashSet<string> _BranchInstructions = new()
		{
			"BCC", "BCS", "BEQ", "BMI", "BNE", "BPL", "BVC", "BVS"
		};

		private static readonly HashSet<string> _ImmediateOnlyInstructions = new()
		{
			"BRK", "CLC", "CLD", "CLI", "CLV", "DEX", "DEY",
			"INX", "INY", "NOP", "PHA", "PHP", "PLA", "PLP",
			"RTI", "RTS", "SEC", "SED", "SEI", "TAX", "TAY",
			"TSX", "TXA", "TXS", "TYA"
		};


		/// <summary>
		/// Assembles a raw assembly program into bytecode.
		/// </summary>
		/// <param name="rawAssembly">The raw assembly code to assemble.</param>
		/// <param name="programCounterStart">The initial memory address pointed to by the program counter.</param>
		/// <returns>The compiled byte code of the passed in assembly program.</returns>
		public static byte[] Assemble(IReadOnlyList<string> rawAssembly)
		{
			_CPU = new CPU_6502();
			_CPU.Initialize("Assembler",false);

			AsmProgram_6502 program = RunPass1(rawAssembly, _CPU.ProgramMemoryStartAddress);
			return RunPass2(program);
		}

		/// <summary>
		/// Runs the first pass over the raw assembly code.
		/// </summary>
		/// <param name="rawAssembly">
		/// The raw assembly code to parse.
		/// This parameter is IReadOnlyList/<string/> since we need to be able to enumerate the list more than once.</param>
		/// If we use IEnumerator/<string/>, the problem is that we can silently fail if the collection is a type that
		/// can only be enumerated once (like a stream reader). This is because the first pass will consume the enumerator,
		/// and the second pass will have nothing left to enumerate.
		/// <returns>The result of the first assembly pass.</returns>
		/// <exception cref="AssemblyException">When an assembly exception occurs.</exception>
		private static AsmProgram_6502 RunPass1(IReadOnlyList<string> rawAssembly, uint programCounterStart)
		{
			var program = new AsmProgram_6502();
			uint programCounter = programCounterStart;
			uint lineNumber = 0;

			foreach (string rawLine in rawAssembly)
			{
				lineNumber++;

				try
				{
					ProgramInstruction_6502 instruction = ParseLine(rawLine, lineNumber, program.Labels, programCounter);
					if (instruction.Name is not null)
					{
						program.Instructions.Add(instruction);
						programCounter += instruction.Size;
					}

					// Lines that only contain a label don't advance the program counter.
				}
				catch (AssemblyException e)
				{
					throw new AssemblyException(e.Message, lineNumber);
				}
			}


			return program;
		}

		private static byte[] RunPass2(AsmProgram_6502 program)
		{
			const int MaxIterations = 16; // This limit prevents infinite loops in case of a bug in the assembler.

			for (int attempt = 1; ; attempt++)
			{
				Readdress(program);

				var output = new List<byte>();
				bool stable = true;
				int programCounter = program.ProgramCounterStart;

				foreach (var instruction in program.Instructions)
				{
					if (instruction.Name is null)
						continue;	// Label only line, so we don't need to emit any bytes.

					byte[] bytes = EmitInstruction(
						programCounter, 
						instruction.Name, 
						instruction.LineNumber,
						instruction.Operand!,
						program.Labels);

					output.AddRange(bytes);
					programCounter += (int) bytes.Length;

					if (bytes.Length != instruction.Size)
					{
						instruction.Size = (byte) bytes.Length;
						stable = false;
					}

				} // end foreach instruction


				if (stable)
					return output.ToArray();

				if (attempt >= MaxIterations)
					throw new AssemblyException(
						"Assembly failed! Instruction sizes did not converge. Aborting to avoid an infinite loop!");

			} // end for int attempt
		}

		/// <summary>
		/// Recalculates the memory address of every label in the program.
		/// </summary>
		/// <param name="program">The program to recalculate all label memory addresses for.</param>
		private static void Readdress(AsmProgram_6502 program)
		{
			program.Labels.Clear();
			int programCounter = program.ProgramCounterStart;

			foreach (var instruction in program.Instructions)
			{
				if (instruction.Label is not null)
					program.Labels[instruction.Label] = programCounter;

				programCounter += instruction.Size;
			}
		}

		private static byte[] EmitInstruction(
			int programCounter, 
			string mnemonic,
			uint lineNumber,
			ParsedOperand_6502 operand,
			IReadOnlyDictionary<string, int> labels)
		{
			ParsedOperand_6502 finalized = FinalizeAddressingMode(mnemonic, lineNumber, operand, labels);

			if (!_CPU.LookUpInstructionByAssemblyNameAndAddressingMode(mnemonic, finalized.AddressingMode, out InstructionDef_6502? instruction))
			{
				throw new AssemblyException($"Line {lineNumber}: {mnemonic} doesn't support {finalized.AddressingMode} addressing mode!");
			}


			switch (finalized.AddressingMode)
			{
				case AddressingModes_6502.Implied:
				case AddressingModes_6502.Accumulator:
					return new[] { instruction!.OpCode }; // The ! here suppresses the warning that instruction may be null here. We already catch that in the if statement above, so it can never be null here.


				case AddressingModes_6502.Immediate:
				case AddressingModes_6502.ZeroPage:
				case AddressingModes_6502.ZeroPage_X:
				case AddressingModes_6502.ZeroPage_Y:
				case AddressingModes_6502.IndexedIndirect: // Also known as Indirect X
				case AddressingModes_6502.IndirectIndexed: // Also known as Indirect Y
				{
					if (!TryEval(finalized.Expression, labels, out int value))
						throw new AssemblyException(
							$"Line {lineNumber}: Can't evaluate expression \"{finalized.Expression}\"");
					return new[] { instruction!.OpCode, (byte)value };
				}


				case AddressingModes_6502.Absolute:
				case AddressingModes_6502.Absolute_X:
				case AddressingModes_6502.Absolute_Y:
				case AddressingModes_6502.Indirect:
				{
					if (!TryEval(finalized.Expression, labels, out int value2))
						throw new AssemblyException(
							$"Line {lineNumber}: Can't evaluate expression \"{finalized.Expression}\"");
					return new[]
					{
						instruction!.OpCode,
						(byte)(value2 & 0xFF),
						(byte)(value2 >> 8)
					};
				}


				case AddressingModes_6502.Relative:
				{
					int target = Eval(finalized.Expression, labels, lineNumber);
					int offset = (int) (target - (programCounter + 2));

					if (offset is < -128 or > 127)
					{
						throw new AssemblyException(
							$"Line {lineNumber}: Branch out of range ({offset}). " +
							"Rewrite as reversed branch over a JMP, or move the code.");
					}

					return new[]
					{
						instruction!.OpCode,
						(byte) (offset & 0xFF) // Two's complement byte (aka, value could be negative).
					};
				}


				default:
					throw new AssemblyException($"Line {lineNumber}: Unsupported addressing mode: {finalized.AddressingMode}");

			} // end switch finalized.AddressingMode
		}

		private static ParsedOperand_6502 FinalizeAddressingMode(
			string mnemonic, 
			uint lineNumber,
			ParsedOperand_6502 operand, 
			IReadOnlyDictionary<string, int> labels)
		{
			// Implied and Accumulator operands carry no expression - nothing to evaluate or
			// finalize here. (The emission switch still evaluates the real expressions.)
			if (operand.Expression.Length == 0)
				return operand;

			int value = Eval(operand.Expression, labels, lineNumber);

			switch (operand.AddressingMode)
			{
				// An absolute address that fits in zero page addressing mode, but only if instruction has a zero page form.
				case AddressingModes_6502.Absolute when value < 0x100 &&
				                                        _CPU.LookUpInstructionByAssemblyNameAndAddressingMode(mnemonic,
					                                        AddressingModes_6502.ZeroPage,
					                                        out InstructionDef_6502? instructionDef):
				{
					// This "with" notation is because operand is a record type, which is immutable.
					return operand with
					{
						AddressingMode = AddressingModes_6502.ZeroPage, 
						Size = 1
					};
				}


				// An indexed operand that outgrew zero page X addressing mode. Promote if the necessary form of the instruction exists.
				case AddressingModes_6502.ZeroPage_X when value >= 0x100 &&
				                                        _CPU.LookUpInstructionByAssemblyNameAndAddressingMode(mnemonic,
					                                        AddressingModes_6502.Absolute_X,
					                                        out InstructionDef_6502? instructionDef):
				{
					// This "with" notation is because operand is a record type, which is immutable.
					return operand with
					{
						AddressingMode = AddressingModes_6502.Absolute_X, 
						Size = 2
					};
				}

				// An indexed operand that outgrew zero page Y addressing mode. Promote if the necessary form of the instruction exists.
				case AddressingModes_6502.ZeroPage_Y when value >= 0x100 &&
														_CPU.LookUpInstructionByAssemblyNameAndAddressingMode(mnemonic,
															AddressingModes_6502.Absolute_Y,
															out InstructionDef_6502? instructionDef):
					{
						// This "with" notation is because operand is a record type, which is immutable.
						return operand with
						{
							AddressingMode = AddressingModes_6502.Absolute_Y, 
							Size = 2
						};
					}


				default:
					return operand;

			} // end switch operand.AddressingMode

		}

		private static ProgramInstruction_6502 ParseLine(string rawLine, uint lineNumber, Dictionary<string, int> labels, uint programCounter)
		{
			// Check if there is a comment on this line.
			string line = _CommentRegex.Replace(rawLine, "").Trim();
			if (line.Length == 0) // Is there anything left after removing the comment?
				return new ProgramInstruction_6502(lineNumber, null, null, null, 0);


			// Check for a leading label - which may be followed by code on the same line
			string? label = null;
			Match labelMatch = _LabelRegex.Match(line);
			if (labelMatch.Success)
			{
				label = labelMatch.Groups[1].Value;
				line = line[labelMatch.Length..].Trim();

				if (!labels.TryAdd(label, (int) programCounter))
					throw new AssemblyException($"Line{lineNumber}: Duplicate label \"{label}\"");
			}

			if (line.Length == 0)
				return new ProgramInstruction_6502(lineNumber, label, null, null, 0);


			// Check instruction syntax.
			Match instructionMatch = _InstructionRegex.Match(line);
			if (!instructionMatch.Success)
				throw new AssemblyException($"Line{lineNumber}: Can't parse \"{line}\"");

			string mnemonic = instructionMatch.Groups[1].Value.ToUpperInvariant();
			string operandText = instructionMatch.Groups[2].Value;

			ParsedOperand_6502 parsedOperand = ParseOperand(operandText, lineNumber);


			// Branches always use RELATIVE mode - meaning a 1-byte operand.
			if (_BranchInstructions.Contains(mnemonic))
			{
				if (parsedOperand.AddressingMode != AddressingModes_6502.Absolute)
					throw new AssemblyException($"Line{lineNumber}: Instruction \"{mnemonic}\" only supports an absolute address!");

				// NOTE: This line may look wrong, since we are setting it to relative mode and not absolute.
				//		 However, this is correct since the encoding will no longer be absolute.
				//		 This is because in 6502, branches don't use a 16-bit target address. Instead, they
				//		 use a signed 1-byte offset relative to the program counter.
				parsedOperand = parsedOperand with { AddressingMode = AddressingModes_6502.Relative, Size = 1 };
			}

			// Implied-only instructions (RTS, CLC, TAX, etc.) never take an operand.
			if (_ImmediateOnlyInstructions.Contains(mnemonic))
			{
				if (operandText.Trim().Length > 0)
					throw new AssemblyException($"Line{lineNumber}: \"{mnemonic}\" takes no operand!");

				parsedOperand = parsedOperand with { AddressingMode = AddressingModes_6502.Implied, Size = 0 };
			}

			// We add one to account for the opcode byte.
			// Also, we have to explicitly cast to byte since the add operator converts the type to int. Another option is adding size += 1 on a 2nd line. That operator has a built-in cast.
			byte size = (byte) (EstimateSize(parsedOperand, labels) + 1); 
			return new ProgramInstruction_6502(lineNumber, label, mnemonic, parsedOperand, size);
		}

		private static ParsedOperand_6502 ParseOperand(string operandText, uint lineNumber)
		{
			string text = operandText.Trim();

			if (text.Length == 0)
				return new ParsedOperand_6502(AddressingModes_6502.Implied, "", 0);

			if (text.Equals("A", StringComparison.OrdinalIgnoreCase))
				return new ParsedOperand_6502(AddressingModes_6502.Accumulator, "", 0);

			if (text[0] == '#')
				return new ParsedOperand_6502(AddressingModes_6502.Immediate, text[1..], 1);

			if (text[0] == '(')
			{
				int closing = text.LastIndexOf(')');
				if (closing < 0)
					throw new AssemblyException($"Line{lineNumber}: Unclosed ()s in \"{operandText}\"");

				string inner = text[1..closing]; // Get the text inside the ()s.
				string suffix = text[(closing + 1)..].Trim();

				if (suffix.Equals(",Y", StringComparison.OrdinalIgnoreCase))
					return new ParsedOperand_6502(AddressingModes_6502.IndirectIndexed, inner, 1);

				if (suffix.Length == 0)
				{
					if (inner.EndsWith(",X", StringComparison.OrdinalIgnoreCase))
						return new ParsedOperand_6502(AddressingModes_6502.IndexedIndirect, inner[..^2], 1);

					return new ParsedOperand_6502(AddressingModes_6502.Indirect, inner, 2);
				}


				throw new AssemblyException($"Line{lineNumber}: Malformed indirect: \"{operandText}\"");
			}


			int comma = text.LastIndexOf(',');
			if (comma >= 0)
			{
				string baseExpression = text[..comma].Trim();
				string index = text[(comma + 1)..].Trim().ToUpperInvariant();

				return index switch
				{
					"X" => new ParsedOperand_6502(AddressingModes_6502.ZeroPage_X, baseExpression, 1),  // provisional
					"Y" => new ParsedOperand_6502(AddressingModes_6502.ZeroPage_Y, baseExpression, 1),  // provisional
					_ => throw new AssemblyException($"Line{lineNumber}: Bad index register: {index}")
				};
			}


			// Bare expression — Zero Page or Absolute, can't tell yet
			return new ParsedOperand_6502(AddressingModes_6502.Absolute, text, 2);  // provisional
		}

		private static int Eval(string expression, IReadOnlyDictionary<string, int> labels, uint lineNumber)
		{
			if (TryEval(expression, labels, out int result))
				return result;

			throw new AssemblyException($"Line{lineNumber}: Can't evaluate expression \"{expression}\"");
		}

		private static bool TryEval(string expression, IReadOnlyDictionary<string, int> labels, out int value)
		{
			value = 0;
			expression = expression.Trim();
			if (expression.Length == 0)
				return false;

			if (expression[0] == '$')   // $FF hex
				return int.TryParse(expression.AsSpan(1), NumberStyles.HexNumber,
					CultureInfo.InvariantCulture, out value);

			if (expression[0] == '%')   // %1010 binary
			{
				foreach (char c in expression.AsSpan(1))
					if (c != '0' && c != '1')
						return false;
				value = Convert.ToInt32(expression[1..], 2);
				return true;
			}

			if (int.TryParse(expression, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
				return true;


			return labels.TryGetValue(expression, out value);  // label (already defined)
		}

		private static byte EstimateSize(ParsedOperand_6502 parsedOperand, IReadOnlyDictionary<string, int> labels)
		{
			if (TryEval(parsedOperand.Expression, labels, out int value))
			{
				if (parsedOperand.AddressingMode == AddressingModes_6502.Absolute &&
				    value < 0x100) // Any address in the first $100 (256) bytes is considered zero-page mode by default.
					return 1;
				if ((parsedOperand.AddressingMode == AddressingModes_6502.ZeroPage_X ||
				     parsedOperand.AddressingMode == AddressingModes_6502.ZeroPage_Y) &&
				     value >= 0x100)
					return 2; // It's actually Absolute,X / Absolute,Y
			}

			return parsedOperand.Size;
		}
	}
}
