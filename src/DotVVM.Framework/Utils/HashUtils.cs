#nullable enable
using System;
using System.Text;

namespace DotVVM.Framework.Utils
{
    internal static class HashUtils
    {
        public static string HashAndBase64Encode(string data) => HashAndBase64Encode(Encoding.Unicode.GetBytes(data));

        public static string HashAndBase64Encode(byte[] data)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha.ComputeHash(data);

                return Convert.ToBase64String(hash);
            }
        }
    }
}
