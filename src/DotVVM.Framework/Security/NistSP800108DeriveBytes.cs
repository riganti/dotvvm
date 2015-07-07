using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Security {
    // For various purposes within DotVVM (ViewModel protection, CSRF blocking...) we need
    // number of cryptographic keys. From crypto perspective it's unwise to reuse symmetric
    // keys and is not feasible to request a whole bunch of them from user.
    //
    // So we need some key derivation function (KDF), to use it for generating specific keys
    // from single specified master one. Sadly, .NET Framework does not offer any key-based
    // KDF (KBKDF), only password-based oned (PBKDF, like Rfc2898DeriveBytes). So I had to
    // implement one. I'm not too happy about writing crypto on this level myself, but there
    // is no other way currently.
    // 
    // The following is implementation of KBKDF according to the NIST SP800-108 standard, 
    // using HMAC as PRF and working in counter mode. See the followinf URL for specification:
    // http://csrc.nist.gov/publications/nistpubs/800-108/sp800-108.pdf

    /// <summary>
    /// This class is implementation of the Key-based Key Derivation Function (KBKDF), as specified in the NIST SP800-108 standard.
    /// </summary>
    internal class NistSP800108DeriveBytes : DeriveBytes {
        private byte[] labelBytes, contextBytes;
        private HMAC pseudoRandomFunction;

        /// <summary>
        /// Initializes a new instance of the NistSP800108DeriveBytes using HMACSHA512 algorithm.
        /// </summary>
        /// <param name="masterKey">The master key to derive from.</param>
        /// <param name="label">The primary purpose string.</param>
        /// <param name="context">The secondary purpose strings.</param>
        public NistSP800108DeriveBytes(byte[] masterKey, string label, string[] context)
            : this(masterKey, label, context, new HMACSHA512()) { }

        /// <summary>
        /// Initializes a new instance of the NistSP800108DeriveBytes using specified algorithm.
        /// </summary>
        /// <param name="masterKey">The master key to derive from.</param>
        /// <param name="label">The primary purpose string.</param>
        /// <param name="context">The secondary purpose strings.</param>
        /// <param name="pseudoRandomFunction">The HMAC function to use as PRF.</param>
        public NistSP800108DeriveBytes(byte[] masterKey, string label, string[] context, HMAC pseudoRandomFunction) {
            // Validate arguments
            if (masterKey == null) throw new ArgumentNullException("masterKey");
            if (masterKey.Length == 0) throw new ArgumentException("The argument cannot be empty.", "masterKey");
            if (label == null) throw new ArgumentNullException("label");
            if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "label");
            if (pseudoRandomFunction == null) throw new ArgumentNullException("pseudoRandomFunction");

            // Setup internal parameters
            this.pseudoRandomFunction = pseudoRandomFunction;
            this.pseudoRandomFunction.Key = masterKey;

            // Convert label and context to byte arrays
            var safeUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            this.labelBytes = safeUtf8.GetBytes(label);
            if(context== null || context.Length > 0) {
                this.contextBytes = new byte[0];
            }
            else {
                using (MemoryStream stream = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(stream, safeUtf8)) {
                    foreach (string item in context) {
                        if (string.IsNullOrWhiteSpace(item)) continue;  // Skip empty context item
                        writer.Write(item);
                    }
                    this.contextBytes = stream.ToArray();
                }
            }
        }

        /// <summary>
        /// Returns pseudo-random key bytes.
        /// </summary>
        /// <param name="cb">Number of bytes to generate</param>
        /// <returns>Returns pseudo-random key bytes.</returns>
        public override byte[] GetBytes(int cb) {
            if (cb < 1) throw new ArgumentOutOfRangeException("cb");
            var cbBits = (uint)cb * 8;

            checked {
                // Initialize buffer to generate new key from. 
                // The buffer structure is:
                //      Step counter    (4 bytes)           sequentially increased counter
                //      Label           (variable length)   primary purpose
                //      0x00            (1 byte)            separator
                //      Context         (variable length)   secondary purpose
                //      Length          (4 bytes)           size of derived key
                var bufferLength = 4 + this.labelBytes.Length + 1 + this.contextBytes.Length + 4;
                var buffer = new byte[bufferLength];
                Buffer.BlockCopy(this.labelBytes, 0, buffer, 4, this.labelBytes.Length);
                if (this.contextBytes.Length > 0) Buffer.BlockCopy(this.contextBytes, 0, buffer, this.labelBytes.Length + 4 + 1, this.contextBytes.Length);
                Buffer.BlockCopy(UInt32ToByteArrayBigEndian(cbBits), 0, buffer, buffer.Length - 4, 4);

                // Calculate output
                // We compute HMAC of changing buffer (increasing the counter part) and join it to generate
                // required number of output bytes
                var bytesRemaining = cb;
                var bytesWritten = 0;
                var output = new byte[cb];
                uint stepCounter = 1;
                while (bytesRemaining > 0) {
                    Buffer.BlockCopy(UInt32ToByteArrayBigEndian(stepCounter), 0, buffer, 0, 4);
                    var stepKey = this.pseudoRandomFunction.ComputeHash(buffer);
                    var bytesCopied = Math.Min(bytesRemaining, stepKey.Length);
                    Buffer.BlockCopy(stepKey, 0, output, bytesWritten, bytesCopied);
                    bytesWritten += bytesCopied;
                    bytesRemaining -= bytesCopied;
                    stepCounter++;
                }
                return output;
            }
        }

        /// <summary>
        /// Resets the state of the operation.
        /// </summary>
        public override void Reset() {
            // NOOP
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (disposing) {
                this.pseudoRandomFunction.Dispose();
            }
        }

        private static byte[] UInt32ToByteArrayBigEndian(uint value) {
            return new byte[] {
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)(value)
            };
        }

    }
}
