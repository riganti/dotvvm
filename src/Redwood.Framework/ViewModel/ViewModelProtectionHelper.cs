using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Redwood.Framework.Configuration;
using Newtonsoft.Json.Bson;

namespace Redwood.Framework.ViewModel
{
    public class ViewModelProtectionHelper
    {
        private byte[] EncryptionKey { get; set; }
        private byte[] MacKey { get; set; }
        public JsonSerializer Serializer { get; set; }

        public ViewModelProtectionHelper(RedwoodSecurityConfiguration config, JsonSerializer serializer = null)
        {
            if (config.EncryptionKey == null) config.EncryptionKey = GenerateRandomKey(32);
            if (config.SigningKey == null) config.SigningKey = GenerateRandomKey(128);
            this.EncryptionKey = config.EncryptionKey;
            this.MacKey = config.SigningKey;
            this.Serializer = serializer ?? new JsonSerializer();
        }

        /// <summary>
        /// Serializes the value and encrypts it.
        /// </summary>
        public virtual string SerializeAndEncrypt(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var encoded = Encoding.UTF8.GetBytes(json);
            var encrypted = EncryptInternal(encoded, 0, encoded.Length);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Decrypts the value and deserializes it.
        /// </summary>
        public virtual object DecryptAndDeserialize(string data, Type type)
        {
            var encrypted = Convert.FromBase64String(data);
            var encoded = DecryptInternal(encrypted, 0, encrypted.Length);
            var json = Encoding.UTF8.GetString(encoded);

            return JsonConvert.DeserializeObject(json, type);
        }

        /// <summary>
        /// Calculates HMAC-SHA512 signature of the value after JSON serialization.
        /// </summary>
        public virtual string CalculateHmacSignature(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var encoded = Encoding.UTF8.GetBytes(json);
            var mac = MacInternal(encoded, 0, encoded.Length);
            return Convert.ToBase64String(mac);
        }

        /// <summary>
        /// Verifies the HMAC signature of the value.
        /// </summary>
        public virtual void VerifyHmacSignature(object obj, string base64Signature)
        {
            var json = JsonConvert.SerializeObject(obj);
            var encoded = Encoding.UTF8.GetBytes(json);
            var mac = Convert.FromBase64String(base64Signature);
            if (!CheckMac(encoded, 0, encoded.Length, mac, 0))
            {
                ThrowSecurityException();
            }
        }

        protected virtual byte[] Serialize(object obj)
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BsonWriter(ms);
                Serializer.Serialize(bw, obj);
                bw.Close();
                return ms.ToArray();
            }
        }

        protected virtual object Deserialize(byte[] data, Type type)
        {
            using (var ms = new MemoryStream(data))
            {
                var br = new BsonReader(ms);
                return Serializer.Deserialize(br, type);
            }
        }

        /// <summary>
        /// Throws the security exception.
        /// </summary>
        protected static void ThrowSecurityException()
        {
            throw new SecurityException("The viewmodel was modified on the client side! The signature is invalid!");
        }
        /// <summary>
        /// Generates cryptographicaly random value of specified length
        /// </summary>
        protected static byte[] GenerateRandomKey(int length)
        {
            var rng = RNGCryptoServiceProvider.Create();
            var b = new byte[length];
            rng.GetBytes(b);
            return b;
        }

        /// <summary>
        /// Encrypts the message using AES CBC mode and adds HMAC SHA512 signature.
        /// </summary>
        protected virtual byte[] EncryptInternal(byte[] message, int offset, int length)
        {
            using (var aes = new RijndaelManaged())
            {
                aes.Key = EncryptionKey;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;

                var encryptor = aes.CreateEncryptor();
                using (var ms = new MemoryStream())
                {
                    // write initialization vector
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    // write encrypted message
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(message, offset, length);
                        cs.FlushFinalBlock();

                        // I can't dispose CryptoStream now, because it will also close the MemoryStream

                        // compute mac of the encrypted data on stream
                        var mac = MacInternal(ms.ToArray(), offset, (int)ms.Position);
                        // and append the mac to result
                        ms.Write(mac, 0, mac.Length);
                    }
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Check the signature and then decrypts the message
        /// </summary>
        protected virtual byte[] DecryptInternal(byte[] cipherText, int offset, int length)
        {
            if (!CheckMac(cipherText))
            {
                ThrowSecurityException();
            }

            using (var aes = new RijndaelManaged())
            {
                aes.Key = EncryptionKey;
                aes.Mode = CipherMode.CBC;
                var iv = new byte[16];

                // read the initialization vector
                for (int i = 0; i < iv.Length; i++)
                {
                    iv[i] = cipherText[i];
                }
                aes.IV = iv;

                using (var plaintext = new MemoryStream())
                {
                    using (var decryptor = aes.CreateDecryptor())
                    {
                        // write the encrypted message
                        using (var cs = new CryptoStream(plaintext, decryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(cipherText, offset + aes.IV.Length, length - aes.IV.Length - 64);
                        }
                    }
                    return plaintext.ToArray();
                }
            }
        }

        /// <summary>
        /// Checks if MAC is correct. Mac is last 64 bytes.
        /// </summary>
        protected virtual bool CheckMac(byte[] message)
        {
            var macIndex = message.Length - 64;
            return CheckMac(message, 0, macIndex, message, macIndex);
        }

        /// <summary>
        /// Check if MAC is correct for message
        /// </summary>
        protected virtual bool CheckMac(byte[] message, int msgOffset, int msgLen, byte[] mac, int macOffset)
        {
            var msgMac = MacInternal(message, msgOffset, msgLen);

            // this has to run exactly the same time in every case to prevent timing attacks
            // I hope that JIT is not optimizing this
            int result = 0;
            for (int i = 0; i < msgMac.Length; i++)
            {
                result |= msgMac[i] ^ mac[macOffset + i];
            }
            return result == 0;
        }

        /// <summary>
        /// Calculates the HMAC SHA512 of the message
        /// </summary>
        protected virtual byte[] MacInternal(byte[] message, int offset, int length)
        {
            using (var m = new HMACSHA512(MacKey))
            {
                return m.ComputeHash(message, offset, length);
            }
        }
    }
}
