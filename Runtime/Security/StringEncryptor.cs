using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Core.Security
{
    public class StringEncryptor
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public StringEncryptor(string key, string iv)
        {
            if (key == null || iv == null)
                throw new ArgumentNullException("Key and IV cannot be null");

            if (key.Length != 32 || iv.Length != 16)
                throw new ArgumentException("Key must be 32 characters and IV must be 16 characters long");

            _key = Encoding.UTF8.GetBytes(key);
            _iv = Encoding.UTF8.GetBytes(iv);
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return "";

            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }

                    var encrypted = ms.ToArray();
                    return Convert.ToBase64String(encrypted);
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return "";

            var cipherBytes = Convert.FromBase64String(cipherText);

            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream(cipherBytes))
                {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (var sr = new StreamReader(cs))
                        {
                            var decrypted = sr.ReadToEnd();
                            return decrypted;
                        }
                    }
                }
            }
        }
    }
}