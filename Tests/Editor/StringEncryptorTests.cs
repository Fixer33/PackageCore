using Core.Security;
using NUnit.Framework;
using UnityEngine;

namespace Core.Editor.Tests
{
    public class StringEncryptorTests
    {
        public static readonly string ENCRYPTION_KEY = "At this point if you know what to do with that - you deserve to hack it :)"[..32];
        public static readonly string ENCRYPTION_IV = "And this is just some random text"[..16];
        
        [Test]
        public void EncryptingAnyLength()
        {
            const int max_length = 3000;
            string original = "", result;
            StringEncryptor encryptor = new StringEncryptor(ENCRYPTION_KEY, ENCRYPTION_IV);
            
            string alphabet = "";
            for (char i = 'a'; i <= 'z'; i++)
            {
                alphabet += i;
                alphabet += i.ToString().ToUpper();
            }
            
            for (int i = 0; i < max_length; i++)
            {
                original += alphabet[Random.Range(0, alphabet.Length)];
                result = encryptor.Encrypt(original);
                Assert.False(result.Equals(original));
            }
            
            Assert.Pass();
        }

        [Test]
        public void DecryptingText()
        {
            const int max_length = 3000;
            string original = "", resultEncrypted, resultDecrypted;
            StringEncryptor encryptor = new StringEncryptor(ENCRYPTION_KEY, ENCRYPTION_IV);
            
            string alphabet = "";
            for (char i = 'a'; i <= 'z'; i++)
            {
                alphabet += i;
                alphabet += i.ToString().ToUpper();
            }
            
            for (int i = 0; i < max_length; i++)
            {
                original += alphabet[Random.Range(0, alphabet.Length)];
                resultEncrypted = encryptor.Encrypt(original);
                resultDecrypted = encryptor.Decrypt(resultEncrypted);
                Assert.IsTrue(resultDecrypted.Equals(original));
            }
            
            Assert.Pass();
        }
    }
}
