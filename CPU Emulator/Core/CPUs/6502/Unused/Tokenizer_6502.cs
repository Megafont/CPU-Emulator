using System;
using System.Collections.Generic;
using System.Text;

namespace CPU_Emulator.Core.CPUs._6502.Assembler
{
	public static class Tokenizer_6502
	{
		private static List<string> _Tokens;

		private static int _CurTokenStartPos = 0;
		private static bool _CurTokenIsAlphaNumeric;

		private static int _CurCharIndex = 0;
		private static char _CurChar;

		private static bool _CurCharIsLastChar = false;
		private static bool _CurCharIsWhiteSpace = false;
		private static bool _CurCharIsPunctuation = false;
		private static bool _CurCharIsLetter = false;
		private static bool _CurCharIsNumber = false;

		private static bool _CurCharIsEndOfToken = false;



		public static List<string> Tokenize(string instruction)
		{
			_Tokens = new();

			// Set this variable early to prevent an extra call to EndToken() from happening when the loop below
			// checks the first character in the instruction string.
			_CurTokenIsAlphaNumeric = char.IsLetterOrDigit(instruction[0]);


			// Iterate through the instruction one char at a time to break it up into tokens.
			for (_CurCharIndex = 0; _CurCharIndex < instruction.Length; _CurCharIndex++)
			{
				_CurChar = instruction[_CurCharIndex];

				_CurCharIsWhiteSpace = char.IsWhiteSpace(_CurChar);
				_CurCharIsLetter = char.IsLetter(_CurChar);
				_CurCharIsNumber = char.IsNumber(_CurChar);
				_CurCharIsPunctuation = char.IsPunctuation(_CurChar);
				_CurCharIsLastChar = _CurCharIndex == instruction.Length - 1;

				bool prevCharIsPunctuation =
					_CurCharIndex > 0 && char.IsPunctuation(instruction[_CurCharIndex - 1]);

				_CurCharIsEndOfToken = _CurCharIsWhiteSpace || _CurCharIsPunctuation || _CurCharIsLastChar || prevCharIsPunctuation;

				//Console.WriteLine($"Char: {_CurChar}    IsWP: {_CurCharIsWhiteSpace}    IsLt: {_CurCharIsLetter}    IsNb: {_CurCharIsNumber}    IsPn: {_CurCharIsPunctuation}    IsLast: {_CurCharIsLastChar}    IsEndOfToken: {_CurCharIsEndOfToken}");

				if (_CurCharIsEndOfToken)
					EndToken(instruction);

			} // end for i


			// If the current character is punctuation and also the last character in the instruction string, then make an extra call to EndToken()
			// to add this closing punctuation character as a token.
			if (_CurCharIsPunctuation && _CurCharIsLastChar)
				EndToken(instruction);

			return _Tokens;
		}

		private static void EndToken(string instruction)
		{
			//Console.WriteLine($"END TOKEN: {_CurChar}");


			string newToken = string.Empty;

			// Extract this token from the instruction string.
			newToken = instruction.Substring(_CurTokenStartPos, _CurCharIndex - _CurTokenStartPos);

			// If this token is empty, then don't add it.
			if (!string.IsNullOrWhiteSpace(newToken))
				_Tokens.Add(newToken);

			// If the current char is a whitespace character, then set curTokenStartPos to _CurCharIndex + 1 so we skip this char.
			// Otherwise, set it to _CurCharIndex.
			_CurTokenStartPos = _CurCharIsWhiteSpace ? _CurCharIndex + 1 : _CurCharIndex;

			_CurTokenIsAlphaNumeric = (_CurCharIndex < instruction.Length - 1 && 
			                           char.IsLetterOrDigit(instruction[_CurCharIndex + 1]));

		}
	}
}
