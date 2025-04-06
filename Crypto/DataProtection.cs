using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using CADProjectsHub.Helpers;
using System.Diagnostics;

namespace CADProjectsHub.Crypto
{
    public class DataProtection
    {
        private readonly CryptoSettings _cryptoSettings;

        public DataProtection(IOptions<CryptoSettings> cryptoSettings)
        {
            _cryptoSettings = cryptoSettings.Value;
        }

        public string Encrypt(string ConstructorName, string Key, out string IVKey)
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
                    aes.KeySize = _cryptoSettings.AESKeySize;
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
                        LogOperation("EncryptConstructor", "AES", aes.KeySize, "StringSize", Encoding.UTF8.GetByteCount(ConstructorName), stopwatch.ElapsedTicks * 1_000_000 / Stopwatch.Frequency);

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

        public string Decrypt(string CipherText, string Key, string IVKey)
        {
            if (string.IsNullOrEmpty(CipherText) || string.IsNullOrEmpty(IVKey))
            {
                return string.Empty;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Padding = PaddingMode.PKCS7;
                    aes.KeySize = _cryptoSettings.AESKeySize;
                    aes.Key = Convert.FromBase64String(Key);
                    aes.IV = Convert.FromBase64String(IVKey);

                    ICryptoTransform decryptor = aes.CreateDecryptor();

                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(CipherText)))
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        string decrypted = sr.ReadToEnd();

                        stopwatch.Stop();
                        LogOperation("DecryptConstructor", "AES", aes.KeySize, "StringSize", Encoding.UTF8.GetByteCount(decrypted), stopwatch.ElapsedTicks * 1_000_000 / Stopwatch.Frequency);

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

        private void LogOperation(string operation, string algorithm, int keySizeBits, string sizeType, int dataSizeBytes, long timeMs)
        {
            try
            {
                string logPath = Path.Combine("wwwroot", "logs", "data_benchmark_log.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                File.AppendAllText(logPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {operation} | {algorithm} | AESKey: {keySizeBits} | {sizeType}: {dataSizeBytes} B | Time: {timeMs} µs{Environment.NewLine}");
            }
            catch
            {
                // Ignoruj błędy logowania
            }
        }
    }
}
