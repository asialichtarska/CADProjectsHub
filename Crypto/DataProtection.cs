using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CADProjectsHub.Crypto
{
    public static class DataProtection
    {
        // Metoda do jednorazowego wygenerowania klucza AES i zapisania go w appsettings.json.
        public static string GenerateKey()
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                return Convert.ToBase64String(aes.Key);
            }
        }

        public static string Encrypt(string ConstructorName, string Key, out string IVKey)
        {
            if (string.IsNullOrEmpty(ConstructorName))
            {
                IVKey = string.Empty;
                return string.Empty;
            }

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = Convert.FromBase64String(Key);
                    aes.GenerateIV();

                    IVKey = Convert.ToBase64String(aes.IV);
                    ICryptoTransform encryptor = aes.CreateEncryptor();

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(ConstructorName);
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                IVKey = string.Empty;
                return string.Empty;
            }
        }

        public static string Decrypt(string CipherText, string Key, string IVKey)
        {
            if (string.IsNullOrEmpty(CipherText) || string.IsNullOrEmpty(IVKey))
            {
                return string.Empty;
            }

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = Convert.FromBase64String(Key);
                    aes.IV = Convert.FromBase64String(IVKey);

                    ICryptoTransform decryptor = aes.CreateDecryptor();

                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(CipherText)))
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return string.Empty;
            }
        }
    }
}