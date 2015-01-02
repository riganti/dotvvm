using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Configuration;

namespace Redwood.Framework.Security {
    // This class is Redwood equivalent of System.Web.Security.MachineKey
    // Supports protection (encryption + signing) of arbitrary data using key derived from
    // global Redwood configuration via NistSP800108DeriveBytes.
    //
    // We use AES-256 in CBC mode signed using HMACSHA512. I would be more comfortable with
    // using authenticated operation modes like GCM/CCM, but .NET Framework does not support
    // it as of now. There is CLR Security project (https://clrsecurity.codeplex.com/), but
    // it does not have NuGet package now and it's future is kind of unclear.

    internal class ApplicationKeyHelper {
        private RedwoodSecurityConfiguration config;

        public ApplicationKeyHelper(RedwoodSecurityConfiguration config) {
            this.config = config;
        }

        public string ProtectString(string s, string label, params string[] context) {
            if (s == null) throw new ArgumentNullException("s");
            if (label == null) throw new ArgumentNullException("label");
            if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "label");

            var data = Encoding.UTF8.GetBytes(s);
            data = this.ProtectData(data, label, context);
            return Convert.ToBase64String(data);
        }

        public byte[] ProtectData(byte[] data, string label, params string[] context) {
            // Validate arguments
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length == 0) throw new ArgumentException("Value cannot me empty.", "data");
            if (label == null) throw new ArgumentNullException("label");
            if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "label");

            // Get derived keys
            byte[] derivedEncKey, derivedSigKey;
            this.GetDerivedKeys(label, context, out derivedEncKey, out derivedSigKey);

            using (var ms = new MemoryStream()) {
                // Prepare AES-256 in CBC mode
                using (var aes = new RijndaelManaged()) {
                    aes.Key = derivedEncKey;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    // Generate 16-byte IV and put it at beginning of output
                    aes.GenerateIV();
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    // Encrypt payload and write it to output
                    using (var enc = aes.CreateEncryptor()) {
                        var encData = enc.TransformFinalBlock(data, 0, data.Length);
                        ms.Write(encData, 0, encData.Length);
                    }
                }

                // Sign IV + encrypted data 
                using (var mac = new HMACSHA512(derivedSigKey)) {
                    var sigData = mac.ComputeHash(ms.ToArray());

                    // Write signature to output (last 64 bytes)
                    ms.Write(sigData, 0, sigData.Length);
                }

                // Return encrypted and signed data
                return ms.ToArray();
            }
        }

        public string UnprotectString(string s, string label, params string[] context) {
            if (s == null) throw new ArgumentNullException("s");
            if (label == null) throw new ArgumentNullException("label");
            if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "label");

            var data = Convert.FromBase64String(s);
            data = this.UnprotectData(data, label, context);
            return Encoding.UTF8.GetString(data);
        }

        public byte[] UnprotectData(byte[] data, string label, params string[] context) {
            // Validate arguments
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length == 0) throw new ArgumentException("Value cannot me empty.", "data");
            if (label == null) throw new ArgumentNullException("label");
            if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "label");

            // Get derived keys
            byte[] derivedEncKey, derivedSigKey;
            this.GetDerivedKeys(label, context, out derivedEncKey, out derivedSigKey);

            // Verify signature
            using (var mac = new HMACSHA512(derivedSigKey)) {
                // Compute signature for all but last 64 bytes of data (that's the old signature)
                var sigData = mac.ComputeHash(data, 0, data.Length - 64);

                // Compare computed signature with supplied one
                var supSigData = new byte[64];
                Buffer.BlockCopy(data, data.Length - 64, supSigData, 0, 64);
                if (!CompareByteArrays(sigData, supSigData)) throw new CryptographicException("Invalid signature. The data is corrupted or was being tampered with.");
            }

            // Decrypt data
            using (var aes = new RijndaelManaged()) {
                // Prepare AES 256
                aes.Key = derivedEncKey;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Read IV (first 16 bytes)
                Buffer.BlockCopy(data, 0, aes.IV, 0, 16);

                // Decrypt rest of data, except last 64 bytes (sig)
                using (var dec = aes.CreateDecryptor()) {
                    var decData = dec.TransformFinalBlock(data, 16, data.Length - 64 - 16);
                    return decData;
                }
            }
        }

        private bool CompareByteArrays(byte[] a, byte[] b) {
            if (a == null) throw new ArgumentNullException("a");
            if (b == null) throw new ArgumentNullException("b");
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++) {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        private void GetDerivedKeys(string label, string[] context, out byte[] encryptionKey, out byte[] signingKey) {
            if (label == null) throw new ArgumentNullException("label");
            if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "label");

            // Derive 256-bit AES encryption key
            using (var kdf = new NistSP800108DeriveBytes(this.config.EncryptionKey, label, context)) {
                encryptionKey = kdf.GetBytes(32);
            }

            // Derive 1024-bit HMACSHA512 signing key
            using (var kdf = new NistSP800108DeriveBytes(this.config.SigningKey, label, context)) {
                signingKey = kdf.GetBytes(128);
            }
        }
    }
}
