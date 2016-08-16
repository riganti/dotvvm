using System;
using System.Security.Cryptography;

namespace DotVVM.Framework.Utils
{
    public static class SecureGuidGenerator
    {
        /// <summary>
        /// Generates a random Guid with the <see cref="RNGCryptoServiceProvider"/>.
        /// The Guid is NOT a valid v4 Guid, that would only lower the security.
        /// </summary>
        public static Guid GenerateGuid()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] bytes = new byte[16];
                rng.GetBytes(bytes);

                return new Guid(bytes);
            }
        }
    }
}
