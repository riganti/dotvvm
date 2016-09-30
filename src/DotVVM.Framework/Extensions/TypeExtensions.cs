using System;
using System.Security.Cryptography;
using System.Text;

namespace DotVVM.Framework
{
    internal static class TypeExtensions
    {
        public static string GetTypeId(this Type type)
        {
            using (var sha1 = SHA1.Create())
            {
                var result = new StringBuilder();
                var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(type.FullName));

                foreach (var b in hashBytes)
                {
                    result.Append(b.ToString("x2"));
                }

                return result.ToString();
            }
        }
    }
}