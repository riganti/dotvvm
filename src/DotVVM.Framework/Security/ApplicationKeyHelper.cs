using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Security {
    // This class is DotVVM equivalent of System.Web.Security.MachineKey
    // Supports protection (encryption + signing) of arbitrary data using key derived from
    // global DotVVM configuration via NistSP800108DeriveBytes.
    //
    // We use AES-256 in CBC mode signed using HMACSHA512. I would be more comfortable with
    // using authenticated operation modes like GCM/CCM, but .NET Framework does not support
    // it as of now. There is CLR Security project (https://clrsecurity.codeplex.com/), but
    // it does not have NuGet package now and it's future is kind of unclear.

    internal class ApplicationKeyHelper {
        private DotvvmSecurityConfiguration config;

        public ApplicationKeyHelper(DotvvmSecurityConfiguration config) {
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

            // Prepare AES-256 in CBC mode
            byte[] iv, encryptedData, signatureData;
            using (var aes = new RijndaelManaged()) {
                aes.Key = derivedEncKey;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Generate 16-byte IV and put it at beginning of output
                aes.GenerateIV();
                iv = aes.IV;

                // Encrypt payload and write it to output
                using (var enc = aes.CreateEncryptor()) {
                    encryptedData = enc.TransformFinalBlock(data, 0, data.Length);
                }
            }

            // Sign IV + encrypted data 
            using (var mac = new HMACSHA512(derivedSigKey)) {
                var dataToSign = new byte[iv.Length + encryptedData.Length];
                iv.CopyTo(dataToSign, 0);
                encryptedData.CopyTo(dataToSign, iv.Length);
                signatureData = mac.ComputeHash(dataToSign);
            }

            // Return encrypted and signed data
            using (var ms = new MemoryStream()) {
                ms.Write(signatureData, 0, signatureData.Length);
                ms.Write(iv, 0, iv.Length);
                ms.Write(encryptedData, 0, encryptedData.Length);
                var outData = ms.ToArray();
                return outData;
            }
        }

        public string UnprotectString(string s, string label, params string[] context) {
            if (s == null) throw new ArgumentNullException("s");
            if (label == null) throw new ArgumentNullException("label");
            if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "label");

            byte[] protectedData;
            try {
                protectedData = Convert.FromBase64String(s);
            }
            catch (Exception ex) {
                throw new CryptographicException("Invalid signature. The data is corrupted or was being tampered with.", ex);
            }
            var data = this.UnprotectData(protectedData, label, context);
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

            // Get data to work with
            var signatureData = new byte[64];   // 512 bits - length of SHA512
            Buffer.BlockCopy(data, 0, signatureData, 0, signatureData.Length);

            var dataToSign = new byte[data.Length - signatureData.Length];
            Buffer.BlockCopy(data, signatureData.Length, dataToSign, 0, dataToSign.Length);

            var iv = new byte[16];  // 128 bits - length of IV in this case
            Buffer.BlockCopy(data, signatureData.Length, iv, 0, iv.Length);

            var encryptedData = new byte[data.Length - signatureData.Length - iv.Length];
            Buffer.BlockCopy(data, signatureData.Length + iv.Length, encryptedData, 0, encryptedData.Length);

            // Verify signature
            using (var mac = new HMACSHA512(derivedSigKey)) {
                // Compute signature
                var expectedSignatureData = mac.ComputeHash(dataToSign);

                // Compare computed signature with supplied one
                if (!expectedSignatureData.SequenceEqual(signatureData)) throw new CryptographicException("Invalid signature. The data is corrupted or was being tampered with.");
            }

            // Decrypt data
            using (var aes = new RijndaelManaged()) {
                // Prepare AES 256
                aes.Key = derivedEncKey;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Read IV (first 16 bytes)
                aes.IV = iv;

                // Decrypt rest of data, except last 64 bytes (sig)
                using (var dec = aes.CreateDecryptor()) {
                    var decryptedData = dec.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                    return decryptedData;
                }
            }
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
