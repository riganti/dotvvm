using System;
using System.IO;
using System.Threading.Tasks;

namespace DotVVM.Framework.Utils
{
    static class MemoryUtils
    {
        public static Span<byte> ToSpan(this MemoryStream stream) =>
            stream.TryGetBuffer(out var buffer) ? buffer.AsSpan().Slice(0, (int)stream.Length) : stream.ToArray();
        public static Memory<byte> ToMemory(this MemoryStream stream) =>
            stream.TryGetBuffer(out var buffer) ? buffer.AsMemory().Slice(0, (int)stream.Length) : stream.ToArray();

        public static ReadOnlySpan<T> Readonly<T>(this Span<T> span) => span;
        public static ReadOnlyMemory<T> Readonly<T>(this Memory<T> span) => span;

        public static Memory<byte> ReadToMemory(this Stream stream)
        {
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            return buffer.ToMemory();
        }
        public static async Task<Memory<byte>> ReadToMemoryAsnc(this Stream stream)
        {
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer);
            return buffer.ToMemory();
        }
    }
}
