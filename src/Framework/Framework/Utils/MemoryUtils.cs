using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DotVVM.Framework.Utils
{
    static class MemoryUtils
    {
        public static Span<byte> ToSpan(this MemoryStream stream) =>
            stream.TryGetBuffer(out var buffer) ? buffer.AsSpan() : stream.ToArray();
        public static Memory<byte> ToMemory(this MemoryStream stream) =>
            stream.TryGetBuffer(out var buffer) ? buffer.AsMemory() : stream.ToArray();

        public static MemoryStream CloneReadOnly(this MemoryStream stream)
        {
            return new MemoryStream(stream.GetBuffer(), 0, (int)stream.Length, false);
        }

        public static ReadOnlySpan<T> Readonly<T>(this Span<T> span) => span;
        public static ReadOnlyMemory<T> Readonly<T>(this Memory<T> span) => span;

        public static Memory<byte> ReadToMemory(this Stream stream)
        {
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            return buffer.ToMemory();
        }
        public static async Task<Memory<byte>> ReadToMemoryAsync(this Stream stream)
        {
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer);
            return buffer.ToMemory();
        }

        public static int CopyTo(this Stream stream, byte[] buffer, int offset)
        {
            var readBytesTotal = 0;

            while (true)
            {
                var maxLength = buffer.Length - readBytesTotal - offset;
                if (maxLength == 0)
                    return readBytesTotal;
                var count = stream.Read(buffer, readBytesTotal + offset, maxLength);
                if (count == 0)
                    return readBytesTotal;

                readBytesTotal += count;
            }
        }
    }
}
