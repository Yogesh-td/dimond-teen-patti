using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace TeenPatti.Encryption
{
    public static class AESCryptography
    {
        const string key = "eGlLQc9QF5uzZfHioQNrXzOcUYx9wKvv";

        public static string Encrypt(string plainText)
        {
            plainText = plainText.Replace(" ", "");
            string iv = Generate_IV();

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        swEncrypt.Write(plainText);

                    return Convert.ToBase64String(msEncrypt.ToArray()) + iv;
                }
            }
        }
        public static string Decrypt(string cipherText)
        {
            string iv = cipherText.Substring(cipherText.Length - 16);

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                try
                {
                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText.Substring(0, cipherText.Length - 16))))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
                catch (CryptographicException ex)
                {
                    // Handle decryption error
                    Debug.LogError("Decryption error: " + ex.Message);
                    return null;
                }
            }
        }

        private static string Generate_IV()
        {
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int counter = 0;

            string result = "";
            while (counter < 16)
            {
                result += characters[UnityEngine.Random.Range(0, characters.Length)];
                counter += 1;
            }
            return result;
        }
        public static string Generate_Random_Key(int length)
        {
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int counter = 0;

            string result = "";
            while (counter < length)
            {
                result += characters[UnityEngine.Random.Range(0, characters.Length)];
                counter += 1;
            }
            return result;
        }
    }
}