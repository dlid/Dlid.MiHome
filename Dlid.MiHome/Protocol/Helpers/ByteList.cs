using Dlid.MiHome.Protocol.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dlid.MiHome.Protocol.Helpers
{
    internal class ByteList : List<byte>, IDisposable
    {
        /// <summary>
        /// Create a byte array containing the concatination of the provided byte arrays
        /// </summary>
        /// <param name="byteLists"></param>
        public ByteList(byte[][] byteLists)
        {
            byteLists.ToList().ForEach(byteArray => AddRange(byteArray));
        }

        /// <summary>
        /// Create a byte array with the given content
        /// </summary>
        /// <param name="data"></param>
        public ByteList(byte[] data)
        {
            AddRange(data);
        }

        /// <summary>
        /// Get little endian int 32 from given position and length
        /// </summary>
        public UInt32 ReadInt32LE(int startIndex, int bytes = 4)
        {
            return ReadInt32(startIndex, bytes, false);
        }

        private UInt32 ReadInt32(int startIndex, int bytes, bool littleEndian)
        {
            Int32 lastByte = bytes - 1;
            if (Count < startIndex + bytes)
                throw new ArgumentOutOfRangeException("startIndex", "Data array is too small to read a " + bytes + "-byte value at offset " + startIndex + ".");
            UInt32 value = 0;
            for (Int32 index = 0; index < bytes; index++)
            {
                Int32 offs = startIndex + (littleEndian ? index : lastByte - index);
                value += (UInt32)(this[offs] << (8 * index));
            }
            return value;
        }

        /// <summary>
        /// Get the MD5 hash for the current array data
        /// </summary>
        /// <returns></returns>
        public byte[] ToMd5()
        {
            return MD5Helper.Hash(ToArray());
        } 

        /// <summary>
        /// Create a new byte array with the concatinated values of the provided arrays
        /// </summary>
        /// <param name="byteLists"></param>
        /// <returns></returns>
        public static ByteList Join(params byte[][] byteLists)
        {
            return (new ByteList(byteLists));
        }

        public ByteList(): base() {}

        /// <summary>
        /// Write the current byte array to a memorystream using a ASCII binary writer
        /// </summary>
        public byte[] ToBinaryASCIIArray()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII))
            {
                writer.Write(ToArray());
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Add the given bytes to the array
        /// </summary>
        /// <param name="values"></param>
        public void Add(params byte[] values)
        {
            AddRange(values);
        }

        /// <summary>
        /// Add the specified value 'count' number of times at the given position
        /// </summary>
        /// <param name="value">The value to add </param>
        /// <param name="count">The number of values to add</param>
        /// <param name="position">Where to add it. If not -1, it will insert the value and not affect the total lenght if possible</param>
        public void Repeat(byte value, int count, int position = -1)
        {
            InsertAt(Enumerable.Repeat(value, count).ToArray(), position);
        }

        // <summary>
        /// Writes Int (Big Endian 32 bits [4 bytes])
        /// </summary>
        /// <param name="value"></param>
        /// <param name="position"></param>
        public void WriteUInt32BE(UInt32 value, int position = -1)
        {
            byte[] intBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            InsertAt(intBytes, position);
        }

        // <summary>
        /// Writes Int (Little Endian 32 bits [4 bytes])
        /// </summary>
        /// <param name="value"></param>
        /// <param name="position"></param>
        public void WriteUInt32LE(UInt32 value, int position = -1)
        {
            byte[] intBytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            InsertAt(intBytes, position);
        }

        /// <summary>
        /// Writes Int (Big Endian 16 bits [2 bytes])
        /// </summary>
        /// <param name="value"></param>
        /// <param name="position"></param>
        public void WriteUInt16BE(UInt16 value, int position = -1)
        {
            byte[] intBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(intBytes);
            }
            InsertAt(intBytes.ToArray(), position);
        }

        /// <summary>
        /// Write the given int as a int16 Big Endian
        /// </summary>
        public void WriteUInt16BE(int value, int position = -1)
        {
            WriteUInt16BE(Convert.ToUInt16(value), position);
        }

        /// <summary>
        /// Write the given int as a int16 Little Endian
        /// </summary>
        public void WriteUInt16LE(int value, int position = -1)
        {
            WriteUInt16LE(Convert.ToUInt16(value), position);
        }

        // <summary>
        /// Writes Int (Little Endian 16 bits [2 bytes])
        /// </summary>
        /// <param name="value"></param>
        /// <param name="position"></param>
        public void WriteUInt16LE(UInt16 value, int position = -1)
        {
            byte[] intBytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(intBytes);
            }
            InsertAt(intBytes.ToArray(), position);
        }

        /// <summary>
        /// Inserts the byte array at the specified location without extending the array length
        /// Will pad with 0x00 if position is greater than length.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="position"></param>
        private void InsertAt(byte[] data, int position)
        {
            if (position > 0)
            {
                if (position > Count - 1)
                {
                    var toInsert = (position ) - Count;
                    Repeat(0x00, toInsert, Count - 1);
                }

                if (position + data.Length > Count)
                {
                    // Add missing items first...
                    var itemsToInsert = (position + data.Length) - Count;
                    byte b = 0x00;
                    AddRange(Enumerable.Repeat(b, itemsToInsert).ToArray());
                }

                int j = 0;
                for (var i = position; i < position + data.Length; i++, j++)
                {
                    this[i] = data[j];
                }
                
            }
            else
            {
                AddRange(data);
            }
        }

        public void Dispose() {}
    }
}
