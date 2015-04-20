using Newtonsoft.Json;
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
        [JsonProperty("signingKey")]
        public byte[] SigningKey { get; set; }

        /// <summary>
        /// Gets or sets base-64 encoded key for encryption (128, 192 or 256 key - used for AES).
        /// </summary>
        [JsonProperty("encryptionKey")]
        public byte[] EncryptionKey { get; set; }

        /// <summary> 
        /// Gets or sets name of HTTP cookie used for Session ID 
        /// </summary> 
        [JsonProperty("sessionIdCookieName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SessionIdCookieName { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodSecurityConfiguration"/> class.
        /// </summary>
        public RedwoodSecurityConfiguration()
        {
            SessionIdCookieName = "_RW_SID";
        }
    }
}