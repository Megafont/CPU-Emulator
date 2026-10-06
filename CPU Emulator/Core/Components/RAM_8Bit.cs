using System;
using System.Collections.Generic;
using System.Text;
using CPU_Emulator.Core.Interfaces.Components;
using CPU_Emulator.Core.Utils;

namespace CPU_Emulator.Core.Components
{
	public class RAM_8Bit : IRAM_8Bit
	{
		public ushort PageSize { get; private set; }
		public ushort PageCount { get; private set; }
		
		/// <summary>
		/// The total byte count of this RAM.
		/// </summary>
		public int TotalByteCount => _Memory.Length;

		private byte[] _Memory;


		public void Initialize(ushort pageSize = 256, ushort pageCount = 256)
		{
			if (pageSize < 1)
				throw new ArgumentException("The page size must be greater than 0!");
			if (pageCount < 1)
				throw new ArgumentException("The page count must be greater than 0!");


			PageSize = pageSize;

			PageCount = pageCount;

			_Memory = new byte[pageCount * pageSize];
		}

		public void Reset()
		{
			Array.Clear(_Memory, 0, _Memory.Length);
		}


		/// <summary>
		/// Copies a block of bytes to the specified position in the RAM.
		/// </summary>
		/// <remarks>
		/// This function does not check if the copy operation will span across page boundaries.
		/// I left it this way to allow it to be more flexible.
		/// </remarks>
		/// <param name="data">The array of bytes to copy into the RAM.</param>
		/// <param name="startIndex">The index to start copying it at in the RAM.</param>
		/// <throws cref="InvalidOperationException">When the copy operation would extend beyond the end of the RAM.</throws>
		public void WriteBlockAt(byte[] data, ushort startIndex)
		{
			if (startIndex + data.Length >= TotalByteCount)
				throw new InvalidOperationException($"The specified startIndex ({startIndex}) plus the data's {data.Length} bytes means this data would extend beyond the end of the RAM ({TotalByteCount})!");

			Array.Copy(data, 0, _Memory, startIndex, data.Length);
		}

		/// <summary>
		/// Copies a block of bytes to the specified position in the specified page of the RAM.
		/// </summary>
		/// <param name="data">The array of bytes to copy into the RAM.</param>
		/// <param name="page">The page to copy the data into.
		/// <param name="startIndex">The index to start copying it at in the specified page.</param>
		/// <throws cref="ArgumentException">When the startIndex parameter is larger than the last byte of a RAM page.
		/// <throws cref="InvalidOperationException">When the copy operation would extend beyond the end of the specified RAM page.</throws>
		public void WriteBlockInPage(byte[] data, ushort page, ushort startIndex = 0)
		{
			if (startIndex >= PageSize)
				throw new ArgumentException($"The specified startIndex ({startIndex}) cannot exceed the PageSize ({PageSize}), as this would put it on the next page!");

			if (startIndex + data.Length >= PageSize)
				throw new InvalidOperationException(
					$"The specified byte array's {data.Length} bytes cannot fit in page {page} with the specified start position of {startIndex}");


			ushort startPos = (ushort) ((PageSize * page) + startIndex);

			Array.Copy(data, 0, _Memory, startPos, data.Length);

		}

		/// <summary>
		/// Returns the specified byte from the specified page.
		/// </summary>
		/// <param name="page">The page index.</param>
		/// <param name="index">The byte to access from this page.</param>
		/// <returns>The byte read from memory.</returns>
		/// <exception cref="ArgumentException">If the passed in index is not valid.</exception>
		public byte ReadByteFromPage(ushort page, byte index)
		{
			if (index >= PageSize)
				throw new ArgumentException($"The byte index must be in the range 0-{PageSize - 1}!");


			return _Memory[(page * PageSize) + index];
		}

		/// <summary>
		/// Reads the specified byte from memory.
		/// </summary>
		/// <param name="page">The page index.</param>
		/// <param name="index">The byte to access from this page.</param>
		/// <param name="value">The byte value to be written to memory.</param>
		public void WriteByteInPage(ushort page, byte index, byte value)
		{
			_Memory[(page * PageSize) + index] = value;
		}

		/// <summary>
		/// Returns the specified ushort from the specified page.
		/// </summary>
		/// <param name="page">The page index.</param>
		/// <param name="index">The index of the first byte of the ushort to access from this page.</param>
		/// <returns>The ushort read from memory.</returns>
		/// <exception cref="ArgumentException">If the passed in index is not valid.</exception>
		public ushort ReadUShortFromPage(ushort page, ushort index)
		{
			if (index >= PageSize - 2)
				throw new ArgumentException($"The byte index must be in the range 0-{PageSize - 2}!");


			return MemoryUtils.ReadUInt16LittleEndian(_Memory, (ushort) ((page * PageSize) + index));
		}

		/// <summary>
		/// Writes the specified ushort to memory at the specified starting address in the specified page.
		/// </summary>
		/// <param name="page">The page index.</param>
		/// <param name="index">The index of the first byte to start writing to in the specified page.</param>
		/// <param name="value">The ushort value to be written to memory.</param>
		/// <exception cref="ArgumentException">If the passed in index is not valid.</exception>
		public void WriteUShortInPage(ushort page, ushort index, ushort value)
		{
			if (index >= PageSize - 2)
				throw new ArgumentException($"The byte index must be in the range 0-{PageSize - 2}!");


			MemoryUtils.WriteUInt16LittleEndian(_Memory, (uint) (page * PageSize) + index, value);
		}

		/// <summary>
		/// Returns the specified ushort from memory.
		/// </summary>
		/// <param name="index">The index of the first byte of the ushort to access from memory.</param>
		/// <returns>The ushort read from memory.</returns>
		/// <exception cref="ArgumentException">If the passed in index is not valid.</exception>
		public ushort ReadUShort(ushort index)
		{
			if (index >= PageSize - 2)
				throw new ArgumentException($"The byte index must be in the range 0-{PageSize - 2}!");


			return MemoryUtils.ReadUInt16LittleEndian(_Memory, index);
		}

		/// <summary>
		/// Returns the specified ushort from memory.
		/// </summary>
		/// <param name="index">The index of the first byte of the ushort to access from memory.</param>
		/// <param name="value">The ushort value to be written to memory.</param>
		/// <exception cref="ArgumentException">If the passed in index is not valid.</exception>
		public void WriteUShort(ushort index, ushort value)
		{
			if (index >= PageSize - 2)
				throw new ArgumentException($"The byte index must be in the range 0-{PageSize - 2}!");


			MemoryUtils.WriteUInt16LittleEndian(_Memory, index, value);
		}

		public void DEBUG_PrintPage(ushort pageIndex, byte bytesPerRow = 32, byte groupSize = 2, char separator = ' ')
		{
			DebugUtils.PrintHexDump(
				_Memory,
				(uint) pageIndex * PageSize,
				PageSize,
				bytesPerRow,
				groupSize,
				separator,
				$"DEBUG MEMORY DUMP: PAGE {pageIndex}"
			);

		}

		public void DEBUG_PrintAllPages(byte bytesPerRow = 32, byte groupSize = 2, char separator = ' ')
		{
			string curLine = string.Empty;
			int groupByteCount = 0;
			int lineByteCount = 0;

			for (uint p = 0; p < PageCount; p++)
			{
				DebugUtils.PrintHexDump(
					_Memory,
					p * PageSize,
					PageSize,
					bytesPerRow,
					groupSize,
					separator,
					$"DEBUG MEMORY DUMP: PAGE {p}");
			}

		}

		public byte this[ushort index]
		{
			get => _Memory[index];
			set => _Memory[index] = value;
		}

		public byte[] this[Range range]
		{
			get
			{
				int start = range.Start.IsFromEnd ? TotalByteCount - range.Start.Value : range.Start.Value;
				int end = range.End.IsFromEnd ? TotalByteCount - range.End.Value : range.End.Value;
				int len = end - start;

				return Slice(start, len);
			}
		}

		/// <summary>
		/// This method is necessary so that we can address a range of memory, just like we can with an array.
		/// For example: array[1..4] will return a new array with the values of array[1], array[2], and array[3].
		/// </summary>
		/// <param name="start"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public byte[] Slice(int start, int length)
		{
			// Wrap a subset of your data into a new instance, or use Span<T>
			byte[] slice = new byte[length];
			Array.Copy(_Memory, start, slice, 0, length);
			return slice;
		}
	}
}
