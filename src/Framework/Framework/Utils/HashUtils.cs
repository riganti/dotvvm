using System;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace DotVVM.Framework.Utils
{
    internal static class HashUtils
    {
        public static string HashAndBase64Encode(string data)
        {
            var utf8Length = Encoding.UTF8.GetByteCount(data);
            byte[]? rentedBuffer = null;
            try
            {
                Span<byte> buffer = utf8Length <= 1024
                    ? stackalloc byte[utf8Length]
                    : (rentedBuffer = ArrayPool<byte>.Shared.Rent(utf8Length)).AsSpan(0, utf8Length);

                Encoding.UTF8.GetBytes(data, buffer);
                return HashAndBase64Encode(buffer);
            }
            finally
            {
                if (rentedBuffer != null)
                    ArrayPool<byte>.Shared.Return(rentedBuffer);
            }
        }

        public static string HashAndBase64Encode(byte[] data) => HashAndBase64Encode(data.AsSpan());
        public static string HashAndBase64Encode(ReadOnlySpan<byte> data)
        {
            Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];
            SHA256.HashData(data, hash);
            return Convert.ToBase64String(hash);
        }
    }
}
