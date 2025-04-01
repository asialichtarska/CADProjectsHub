using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CADProjectsHub.Crypto
{
    public static class DataProtection
    {
        private static readonly string LogPath = Path.Combine("wwwroot", "logs", "data_benchmark_log.txt");

        public static string GenerateKey()
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 128;
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
            var stopwatch = Stopwatch.StartNew();
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
                        stopwatch.Stop();
                        LogOperation("EncryptConstructor", "AES", aes.KeySize, "StringSize", Encoding.UTF8.GetByteCount(ConstructorName), stopwatch.ElapsedMilliseconds);

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
            var stopwatch = Stopwatch.StartNew();

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
                        string decrypted = sr.ReadToEnd();

                        stopwatch.Stop();
                        LogOperation("DecryptConstructor", "AES", aes.KeySize, "StringSize", CipherText.Length, stopwatch.ElapsedMilliseconds);

                        return decrypted;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return string.Empty;
            }
        }

        private static void LogOperation(string operation, string algorithm, int aesKeyBits, string sizeType, int dataSizeBytes, long timeMs)
        {
            try
            {
                var logDir = Path.GetDirectoryName(LogPath);
                if (!string.IsNullOrEmpty(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                File.AppendAllText(LogPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {operation} | {algorithm} | AESKey: {aesKeyBits} | {sizeType}: {dataSizeBytes} B | Time: {timeMs} ms{Environment.NewLine}");
            }
            catch
            {
                // Ignoruj błędy logowania
            }
        }
    }
}