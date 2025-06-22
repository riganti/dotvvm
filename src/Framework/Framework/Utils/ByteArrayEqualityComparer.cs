using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DotVVM.Framework.Utils
{
    class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        private ByteArrayEqualityComparer() { }
        public static readonly ByteArrayEqualityComparer Instance  = new ByteArrayEqualityComparer();
        public bool Equals(byte[]? x, byte[]? y) =>
            x == (object?)y ||
            (x is { } && y is { } &&
             x.Length == y.Length &&
             MemoryExtensions.SequenceEqual(x.AsSpan(), y.AsSpan()));

        public int GetHashCode([DisallowNull] byte[] obj) =>
            (int)System.IO.Hashing.Crc32.HashToUInt32(obj.AsSpan());
    }
}
