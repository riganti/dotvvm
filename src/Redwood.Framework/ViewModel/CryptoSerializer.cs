using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.ViewModel
{
    public static class CryptoSerializer
    {
        // TODO: do some better key store
        private static byte[] EncryptionKey = GenerateKey(256 / 8);
        private static byte[] MacKey = GenerateKey(256 / 8);

        private static byte[] GenerateKey(int len)
        {
            var rng = RNGCryptoServiceProvider.Create();
            var b = new byte[len];
            rng.GetBytes(b);
            return b;
        }

        public static string EncryptSerialize(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var encoded = Encoding.UTF8.GetBytes(json);
            var encrypted = EncryptInternal(encoded, 0, encoded.Length);
            return Convert.ToBase64String(encrypted);
        }

        public static object DecryptDeserialize(string data, Type type = null)
        {
            var encrypted = Convert.FromBase64String(data);
            var encoded = DecryptInternal(encrypted, 0, encrypted.Length);
            var json = Encoding.UTF8.GetString(encoded);

            if (type == null) return JsonConvert.DeserializeObject(json);
            else return JsonConvert.DeserializeObject(json, type);
        }

        /// <summary>
        /// returns base64 encoded HMAC SHA512 MAC of object
        /// </summary>
        public static string MacSerialize(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var encoded = Encoding.UTF8.GetBytes(json);
            var mac = MacInternal(encoded, 0, encoded.Length);
            return Convert.ToBase64String(mac);
        }

        public static void CheckObjectMac(object obj, string macb64)
        {
            var json = JsonConvert.SerializeObject(obj);
            var encoded = Encoding.UTF8.GetBytes(json);
            var mac = Convert.FromBase64String(macb64);
            if (!CheckMac(encoded, 0, encoded.Length, mac, 0)) throw new CryptographicException("invalid mac");
        }

        /// <summary>
        /// Encrypts the message using AES CBC mode and adds HMAC SHA512 mac
        /// </summary>
        /// <returns></returns>
        static byte[] EncryptInternal(byte[] message, int offset, int length)
        {
            using (var aes = new RijndaelManaged())
            {
                aes.Key = EncryptionKey;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;

                var encryptor = aes.CreateEncryptor();
                using (var ms = new MemoryStream())
                {
                    // write Initialization vector
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
        /// check message mac and then decrypts
        /// </summary>
        static byte[] DecryptInternal(byte[] cipherText, int offset, int length)
        {
            if (!CheckMac(cipherText)) throw new CryptographicException("mac of message is incorrect");

            using (var aes = new RijndaelManaged())
            {
                aes.Key = EncryptionKey;
                aes.Mode = CipherMode.CBC;
                var iv = new byte[16];
                // read Initialization vector
                for (int i = 0; i < iv.Length; i++)
                {
                    iv[i] = cipherText[i];
                }
                aes.IV = iv;

                using (var plaintext = new MemoryStream())
                {
                    using (var decryptor = aes.CreateDecryptor())
                    {
                        // write encrypted message
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
        /// checks if mac is correct. Mac is the 64 least bytes
        /// this method should run in the same time in every case
        /// </summary>
        static bool CheckMac(byte[] message)
        {
            var macIndex = message.Length - 64;
            var mac = MacInternal(message, 0, macIndex);

            return CheckMac(message, 0, macIndex, message, macIndex);
        }

        /// <summary>
        /// Check if mac is correct for message
        /// </summary>
        static bool CheckMac(byte[] message, int msgOffset, int msgLen, byte[] mac, int macOffset)
        {
            var msgMac = MacInternal(message, msgOffset, msgLen);

            int result = 0;
            for (int i = 0; i < msgMac.Length; i++)
            {
                // TODO: check if compiler is not optimizing this
                // this should switch some bits in result to 1, iff mac is wrong
                result |= msgMac[i] ^ mac[macOffset + i];
            }
            return result == 0;
        }

        /// <summary>
        /// gets the HMAC SHA512 mac of the message
        /// </summary>
        static byte[] MacInternal(byte[] message, int offset, int length)
        {
            using (var m = new HMACSHA512(MacKey))
            {
                return m.ComputeHash(message, offset, length);
            }
        }
    }
}
