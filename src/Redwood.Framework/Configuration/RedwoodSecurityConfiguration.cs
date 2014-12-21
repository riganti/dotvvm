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
        /// Gets or sets base-64 encoded 256-bit key for signing.
        /// </summary>
        public string SigningKey { get; set; }

        /// <summary>
        /// Gets or sets base-64 encoded 256-bit key for encryption.
        /// </summary>
        public string EncryptionKey { get; set; }

    }
}