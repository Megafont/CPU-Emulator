using System;
using System.Collections.Generic;
using System.Text;

namespace CPU_Emulator.Core.Interfaces.Components
{
	public interface IRAM_8Bit
	{
		ushort PageSize { get; }
		ushort PageCount { get; }
		int TotalByteCount { get; }

		void Initialize(ushort pageSize = 256, ushort pageCount = 256);


		byte this[ushort index] { get; set; }
		byte[] this[Range range] { get; }

		byte ReadByteFromPage(ushort page, byte index);
		void WriteByteInPage(ushort page, byte index, byte value);

		ushort ReadUShortFromPage(ushort page, ushort index);
		void WriteUShortInPage(ushort page, ushort index, ushort value);

		ushort ReadUShort(ushort index);
		void WriteUShort(ushort index, ushort value);


		void WriteBlockAt(byte[] data, ushort startIndex);
		void WriteBlockInPage(byte[] data, ushort page, ushort startIndex);

		void Reset();


		void DEBUG_PrintPage(ushort pageIndex, byte bytesPerRow = 32, byte groupSize = 2, char separator = ' ');
		void DEBUG_PrintAllPages(byte bytesPerRow = 32, byte groupSize = 2, char separator = ' ');
	}
}
