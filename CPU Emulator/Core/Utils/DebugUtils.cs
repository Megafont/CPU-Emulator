using System;
using System.Collections.Generic;
using System.Text;

namespace CPU_Emulator.Core.Utils
{
	public static class DebugUtils
	{
		/// <summary>
		/// Prints a hex dump of the specified memory to the console.
		/// </summary>
		/// <param name="memory">The memory array to access.</param>
		/// <param name="startIndex">The index to start dumping at.</param>
		/// <param name="length">The number of bytes to dump.</param>
		/// <param name="bytesPerRow">How many bytes to print on each row.</param>
		/// <param name="groupSize">How many bytes per group.</param>
		/// <param name="separator">The char to print between byte groups.</param>
		public static void PrintHexDump(
			byte[] memory, 
			uint startIndex, 
			uint length, 
			byte bytesPerRow = 32, 
			byte groupSize = 2, 
			char separator = ' ',
			string title = "DEBUG MEMORY DUMP:")
		{
			string curLine = string.Empty;
			int groupByteCount = 0;
			int lineByteCount = 0;

			Console.WriteLine(new string('-', 256));
			Console.WriteLine(title);
			Console.WriteLine(new string('-', 256));

			for (uint i = startIndex; i < startIndex + length; i++)
			{
				curLine += $"{memory[i]:X2}";
				groupByteCount++;
				lineByteCount++;

				if (groupByteCount == groupSize)
				{
					curLine += separator;
					groupByteCount = 0;
				}

				if (lineByteCount == bytesPerRow)
				{
					Console.WriteLine(curLine);
					lineByteCount = 0;
					curLine = string.Empty;
				}

			} // end for i

			Console.WriteLine(new string('-', 256));

		}
	}
}
