using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace FileService.Common
{
    /// <summary>
    /// Szyfrowanie i odszyfrowywanie tekstów
    /// </summary>
    public class CryptoHelper
    {
        /// <summary>
        /// Zaszyfruj tekst
        /// </summary>
        /// <param name="key">Klucz szyfrowania</param>
        /// <param name="plainText">Tekst do zaszyfrowania</param>
        /// <returns>Zaszyfrowany tekst</returns>
        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var memoryStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (var streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }
                    array = memoryStream.ToArray();
                }
            }
            return Convert.ToBase64String(array);
        }

        /// <summary>
        /// Odszyfruj tekst
        /// </summary>
        /// <param name="key">Klucz szyfrowania</param>
        /// <param name="cipherText">Zaszyfrowany tekst</param>
        /// <returns>Odszyfrowany tekst</returns>
        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (var streamReader = new StreamReader(cryptoStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}
