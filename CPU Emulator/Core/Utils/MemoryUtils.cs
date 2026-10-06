using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace CPU_Emulator.Core.Utils
{
	public static class MemoryUtils
	{
		#region BitManipulation

		public static bool GetBit(byte value, byte index)
		{
			if (index > 7)
				throw new ArgumentOutOfRangeException($"{nameof(index)} must be in the range 0-7!");

			return (value & GetBitMask(index)) > 0;
		}

		public static void WriteBit(ref byte parentByte, byte index, bool value)
		{
			if (index > 7)
				throw new ArgumentOutOfRangeException($"{nameof(index)} must be in the range 0-7!");

			if (value)
				SetBit(ref parentByte, index);
			else
				ClearBit(ref parentByte, index);
		}

		public static void SetBit(ref byte parentByte, byte index)
		{
			if (index > 7)
				throw new ArgumentOutOfRangeException($"{nameof(index)} must be in the range 0-7!");

			parentByte |= GetBitMask(index);
		}

		public static void ClearBit(ref byte parentByte, byte index)
		{
			if (index > 7)
				throw new ArgumentOutOfRangeException($"{nameof(index)} must be in the range 0-7!");

			parentByte &= GetInvertedBitMask(index);
		}

		public static void FlipBit(ref byte parentByte, byte index)
		{
			if (index > 7)
				throw new ArgumentOutOfRangeException($"{nameof(index)} must be in the range 0-7!");

			parentByte ^= GetBitMask(index);
		}

		/// <summary>
		/// Gets a bit mask for the bit at the specified index.
		/// </summary>
		/// <param name="index">The bit (0-7) to get the bit mask for.</param>
		/// <returns>The bit mask for the specified bit.</returns>
		public static byte GetBitMask(byte index)
		{
			if (index > 7)
				throw new ArgumentOutOfRangeException($"{nameof(index)} must be in the range 0-7!");

			return (byte) (1 << index);
		}

		public static byte GetInvertedBitMask(byte index)
		{
			if (index > 7)
				throw new ArgumentOutOfRangeException($"{nameof(index)} must be in the range 0-7!");

			byte mask = GetBitMask(index);
			FlipAllBits(ref mask);
			return mask;
		}

		public static void FlipAllBits(ref byte parentByte)
		{
			parentByte = (byte) ~parentByte;
		}

		#endregion



		#region Reading

		public static short ReadInt16BigEndian(byte[] data, uint startIndex)
		{
			if (data == null)
				throw new ArgumentException("The byte array can't be null!");
			if (data.Length < 2)
				throw new ArgumentException("The byte array must contain at least 2 bytes!");
			if (startIndex < 0 || startIndex > data.Length - 2)
				throw new ArgumentException("The start index must be 0 or great and less than byte array length - 2!");


			return BinaryPrimitives.ReadInt16BigEndian(data.AsSpan((int) startIndex));
		}

		public static void WriteInt16BigEndian(byte[] data, uint startIndex, short value)
		{
			if (data == null)
				throw new ArgumentException("The byte array can't be null!");
			if (data.Length < 2)
				throw new ArgumentException("The byte array must contain at least 2 bytes!");
			if (startIndex < 0 || startIndex > data.Length - 2)
				throw new ArgumentException("The start index must be 0 or great and less than byte array length - 2!");


			BinaryPrimitives.WriteInt16BigEndian(data.AsSpan((int)startIndex), value);
		}

		public static ushort ReadUInt16BigEndian(byte[] data, uint startIndex)
		{
			if (data == null)
				throw new ArgumentException("The byte array can't be null!");
			if (data.Length < 2)
				throw new ArgumentException("The byte array must contain at least 2 bytes!");
			if (startIndex < 0 || startIndex > data.Length - 2)
				throw new ArgumentException("The start index must be 0 or great and less than byte array length - 2!");


			return BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan((int) startIndex));
		}
		public static void WriteUInt16BigEndian(byte[] data, uint startIndex, ushort value)
		{
			if (data == null)
				throw new ArgumentException("The byte array can't be null!");
			if (data.Length < 2)
				throw new ArgumentException("The byte array must contain at least 2 bytes!");
			if (startIndex < 0 || startIndex > data.Length - 2)
				throw new ArgumentException("The start index must be 0 or great and less than byte array length - 2!");


			BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan((int)startIndex), value);
		}

		// NOTE: The 6502 is little-endian, so every 16-bit value the CPU reads or writes
		// (instruction operands, RAM pointers, etc.) must use these LittleEndian methods.
		public static ushort ReadUInt16LittleEndian(byte[] data, uint startIndex)
		{
			if (data == null)
				throw new ArgumentException("The byte array can't be null!");
			if (data.Length < 2)
				throw new ArgumentException("The byte array must contain at least 2 bytes!");
			if (startIndex < 0 || startIndex > data.Length - 2)
				throw new ArgumentException("The start index must be 0 or great and less than byte array length - 2!");

			return BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan((int) startIndex));
		}

		public static void WriteUInt16LittleEndian(byte[] data, uint startIndex, ushort value)
		{
			if (data == null)
				throw new ArgumentException("The byte array can't be null!");
			if (data.Length < 2)
				throw new ArgumentException("The byte array must contain at least 2 bytes!");
			if (startIndex < 0 || startIndex > data.Length - 2)
				throw new ArgumentException("The start index must be 0 or great and less than byte array length - 2!");

			BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan((int)startIndex), value);
		}
		#endregion




		#region Conversion

		public static string ToBitString(this byte value)
		{
			return Convert.ToString(value, 2).PadLeft(8, '0');
		}

		public static string ToBitString(this sbyte value)
		{
			return Convert.ToString(value, 2).PadLeft(8, '0');
		}

		public static string ToBitString(this short value)
		{
			return Convert.ToString(value, 2).PadLeft(16, '0');
		}

		public static string ToBitString(this ushort value)
		{
			return Convert.ToString(value, 2).PadLeft(16, '0');
		}

		public static string ToBitString(this int value)
		{
			return Convert.ToString(value, 2).PadLeft(32, '0');
		}

		public static string ToBitString(this uint value)
		{
			return Convert.ToString(value, 2).PadLeft(32, '0');
		}

		public static string ToBitString(this long value)
		{
			return Convert.ToString(value, 2).PadLeft(64, '0');
		}

		public static string ToBitString(this ulong value)
		{
			return Convert.ToString((long) value, 2).PadLeft(64, '0');
		}


		public static string ToHexString(this byte value)
		{
			return Convert.ToString(value, 16).PadLeft(2, '0');
		}

		public static string ToHexString(this sbyte value)
		{
			return Convert.ToString(value, 16).PadLeft(2, '0');
		}

		public static string ToHexString(this short value)
		{
			return Convert.ToString(value, 16).PadLeft(4, '0');
		}

		public static string ToHexString(this ushort value)
		{
			return Convert.ToString(value, 16).PadLeft(4, '0');
		}

		public static string ToHexString(this int value)
		{
			return Convert.ToString(value, 16).PadLeft(8, '0');
		}

		public static string ToHexString(this uint value)
		{
			return Convert.ToString(value, 16).PadLeft(8, '0');
		}

		public static string ToHexString(this long value)
		{
			return Convert.ToString(value, 16).PadLeft(16, '0');
		}

		public static string ToHexString(this ulong value)
		{
			return Convert.ToString((long)value, 16).PadLeft(16, '0');
		}

		#endregion



		#region Misc

		/// <summary>
		/// Checks the endianness of the system this program is running on.
		/// </summary>
		/// <remarks>
		/// NOTE: Windows is generally little endian. You have to make sure you write your own bytes in the correct order regardless of that,
		///		  as Windows doesn't care how you manage your own memory.
		/// </remarks>
		/// <returns>True if the system this program is running on is little endian, or false otherwise.</returns>
		public static bool IsSystemLittleEndian()
		{
			return BitConverter.IsLittleEndian;
		}

		#endregion

	}
}
