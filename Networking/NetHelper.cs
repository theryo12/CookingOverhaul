using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CookingOverhaul.Networking
{
    /// <summary>
    ///     Provides a set of methods for efficient network data serialization, deserialization, and memory management.
    /// </summary>
    /// <remarks>
    ///     This class focuses on high-performance, low-allocation network data handling. It includes support for unmanaged types and common network operations
    ///     to reduce boilerplate and improve maintainability.
    /// </remarks>
    public static class NetHelper
    {
        /// <summary>
        ///     Writes a value of type <typeparamref name="T"/> to a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to write. Must be unmanaged.</typeparam>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="value">The value to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write< T >(BinaryWriter writer, in T value) where T : unmanaged
        {
            ArgumentNullException.ThrowIfNull(writer);

            Span< byte > buffer = stackalloc byte[Unsafe.SizeOf< T >()];
            MemoryMarshal.Write(buffer, in Unsafe.AsRef(in value));
            writer.Write(buffer);
        }
        
        /// <summary>
        ///     Reads a value of type <typeparamref name="T"/> from a <see cref="BinaryReader"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to read. Must be unmanaged.</typeparam>
        /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
        /// <returns>The value read from the stream.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is null.</exception>
        /// <exception cref="EndOfStreamException">Thrown if fewer bytes are read than expected.</exception>
        public static T Read< T >(BinaryReader reader) where T : unmanaged
        {
            ArgumentNullException.ThrowIfNull(reader);

            Span< byte > buffer    = stackalloc byte[Unsafe.SizeOf< T >()];
            var          bytesRead = reader.Read(buffer);
            FillBuffer(reader, buffer);
            return MemoryMarshal.Read< T >(buffer);
        }
        
        /// <summary>
        ///     Writes a span of bytes to a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="data">The span of bytes to write.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="writer"/> is null.</exception>
        public static void WriteBytes(BinaryWriter writer, ReadOnlySpan< byte > data)
        {
            ArgumentNullException.ThrowIfNull(writer);
            
            writer.Write(data.Length);
            writer.Write(data);
        }
        
        /// <summary>
        ///     Reads a span of bytes from a <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
        /// <returns>A byte array containing the read data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is null.</exception>
        /// <exception cref="EndOfStreamException">Thrown if fewer bytes are read than expected.</exception>
        public static byte[] ReadBytes(BinaryReader reader)
        {
            ArgumentNullException.ThrowIfNull(reader);

            var length = reader.ReadInt32();
            if (length < 0)
            {
                throw new InvalidDataException("Invalid length for byte array.");
            }
            var data = ArrayPool< byte >.Shared.Rent(length);
            FillBuffer(reader, data.AsSpan(0, length));
            return data;
        }
        
        /// <summary>
        ///     Serializes an array of unmanaged types to a byte array.
        /// </summary>
        /// <typeparam name="T">The type of the array elements. Must be unmanaged.</typeparam>
        /// <param name="values">The array of values to serialize.</param>
        /// <returns>A byte array containing the serialized data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="values"/> is null.</exception>
        public static byte[] SerializeArray< T >(T[] values) where T : unmanaged
        {
            ArgumentNullException.ThrowIfNull(values);

            var size   = Unsafe.SizeOf< T >() * values.Length;
            var result = GC.AllocateUninitializedArray< byte >(size);
            var span   = result.AsSpan();
            foreach (var t in values)
            {
                MemoryMarshal.Write(span, in t);
                span = span [ Unsafe.SizeOf< T >().. ];
            }
            return result;
        }
        
        /// <summary>
        ///     Deserializes a byte array into an array of unmanaged types.
        /// </summary>
        /// <typeparam name="T">The type of the array elements. Must be unmanaged.</typeparam>
        /// <param name="data">The byte array to deserialize.</param>
        /// <returns>An array of values of type <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is null.</exception>
        public static T[] DeserializeArray< T >(byte[] data) where T : unmanaged
        {
            ArgumentNullException.ThrowIfNull(data);

            var count  = data.Length / Unsafe.SizeOf< T >();
            var result = new T[count];
            var span   = data.AsSpan();
            for (var i = 0; i < count; i++)
            {
                result[i] = MemoryMarshal.Read< T >(span);
                span      = span [ Unsafe.SizeOf< T >().. ];
            }
            return result;
        }
        
        /// <summary>
        ///     Rents a byte array from the <see cref="ArrayPool{T}"/> and fills it with data from the reader.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
        /// <param name="size">The size of the byte array to rent.</param>
        /// <returns>A rented byte array containing the read data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is null.</exception>
        /// <exception cref="EndOfStreamException">Thrown if fewer bytes are read than expected.</exception>
        public static byte[] RentAndRead(BinaryReader reader, int size)
        {
            ArgumentNullException.ThrowIfNull(reader);
            ArgumentOutOfRangeException.ThrowIfNegative(size);

            var buffer = ArrayPool< byte >.Shared.Rent(size);
            FillBuffer(reader, buffer.AsSpan(0, size));
            return buffer;
        }
        
        /// <summary>
        ///     Releases a rented array back to the <see cref="ArrayPool{T}"/>.
        /// </summary>
        /// <param name="buffer">The rented buffer to return.</param>
        public static void ReturnBuffer(byte[] buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            
            ArrayPool<byte>.Shared.Return(buffer);
        }
        
        /// <summary>
        ///     Fills the provided span with data from the <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="buffer">The buffer to fill.</param>
        /// <exception cref="EndOfStreamException">Thrown if fewer bytes are readed than expected.</exception>
        private static void FillBuffer(BinaryReader reader, Span< byte > buffer)
        {
            var totalRead = 0;
            while (totalRead < buffer.Length)
            {
                var bytesRead = reader.Read(buffer [ totalRead.. ]);
                if (bytesRead == 0)
                {
                    throw new EndOfStreamException($"Expected {buffer.Length} bytes but only read {bytesRead} instead.");
                }
                totalRead += bytesRead;
            }
        }
    }
}
