using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Redwood.Framework.Configuration;

namespace Redwood.Framework.ViewModel
{
    public static class ViewModelProtectionHelper
    {

        internal static RedwoodConfiguration Configuration { get; set; }


        private static byte[] EncryptionKey
        {
            get { return Convert.FromBase64String(Configuration.Security.EncryptionKey); }
        }

        private static byte[] MacKey
        {
            get { return Convert.FromBase64String(Configuration.Security.SigningKey); }
        }


        /// <summary>
        /// Serializes the value and encrypts it.
        /// </summary>
        public static string SerializeAndEncrypt(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var encoded = Encoding.UTF8.GetBytes(json);
            var encrypted = EncryptInternal(encoded, 0, encoded.Length);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Decrypts the value and deserializes it.
        /// </summary>
        public static object DecryptAndDeserialize(string data, Type type)
        {
            var encrypted = Convert.FromBase64String(data);
            var encoded = DecryptInternal(encrypted, 0, encrypted.Length);
            var json = Encoding.UTF8.GetString(encoded);

            return JsonConvert.DeserializeObject(json, type);
        }

        /// <summary>
        /// Calculates HMAC-SHA512 signature of the value after JSON serialization.
        /// </summary>
        public static string CalculateHmacSignature(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var encoded = Encoding.UTF8.GetBytes(json);
            var mac = MacInternal(encoded, 0, encoded.Length);
            return Convert.ToBase64String(mac);
        }

        /// <summary>
        /// Verifies the HMAC signature of the value.
        /// </summary>
        public static void VerifyHmacSignature(object obj, string base64Signature)
        {
            var json = JsonConvert.SerializeObject(obj);
            var encoded = Encoding.UTF8.GetBytes(json);
            var mac = Convert.FromBase64String(base64Signature);
            if (!CheckMac(encoded, 0, encoded.Length, mac, 0))
            {
                ThrowSecurityException();
            }
        }

        /// <summary>
        /// Throws the security exception.
        /// </summary>
        private static void ThrowSecurityException()
        {
            throw new SecurityException("The viewmodel was modified on the client side! The signature is invalid!");
        }

        /// <summary>
        /// Encrypts the message using AES CBC mode and adds HMAC SHA512 signature.
        /// </summary>
        private static byte[] EncryptInternal(byte[] message, int offset, int length)
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

                        // write mac
                        var mac = MacInternal(ms.ToArray(), offset, (int)ms.Position);
                        ms.Write(mac, 0, mac.Length);
                    }
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Check the signature and then decrypts the message
        /// </summary>
        private static byte[] DecryptInternal(byte[] cipherText, int offset, int length)
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
        private static bool CheckMac(byte[] message)
        {
            var macIndex = message.Length - 64;
            return CheckMac(message, 0, macIndex, message, macIndex);
        }

        /// <summary>
        /// Check if MAC is correct for message
        /// </summary>
        private static bool CheckMac(byte[] message, int msgOffset, int msgLen, byte[] mac, int macOffset)
        {
            var msgMac = MacInternal(message, msgOffset, msgLen);

            for (int i = 0; i < msgMac.Length; i++)
            {
                if (msgMac[i] != mac[macOffset + i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Calculates the HMAC SHA512 of the message
        /// </summary>
        private static byte[] MacInternal(byte[] message, int offset, int length)
        {
            using (var m = new HMACSHA512(MacKey))
            {
                return m.ComputeHash(message, offset, length);
            }
        }

    }
}
