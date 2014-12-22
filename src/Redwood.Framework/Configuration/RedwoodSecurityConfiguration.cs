using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Configuration
{
    /// <summary>
    /// Contains the encryption keys for ViewModel protection.
    /// </summary>
    public class RedwoodSecurityConfiguration
    {

        /// <summary>
        /// Gets or sets base-64 encoded key for signing (128 bytes recommended - for HMACSHA512, long is hashed or short is padded).
        /// </summary>
        public byte[] SigningKey { get; set; }

        /// <summary>
        /// Gets or sets base-64 encoded key for encryption (128, 192 or 256 key - used for AES).
        /// </summary>
        public byte[] EncryptionKey { get; set; }

    }
}