using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CADProjectsHub.Crypto
{
    public static class CryptoBenchmark
    {
        private static readonly string LogPath = Path.Combine("wwwroot", "logs", "crypto_benchmark_log.txt");

        public static void RunAllTests()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath));
            File.WriteAllText(LogPath, "===== Crypto Benchmark Results =====\n\n");

            TestAES(128);
            TestAES(192);
            TestAES(256);

            TestRSA(1024);
            TestRSA(2048);
            TestRSA(4096);
        }

        private static void TestAES(int keySize)
        {
            Aes aes = Aes.Create();
            aes.KeySize = keySize;
            aes.GenerateKey();
            aes.GenerateIV();

            var data = Encoding.UTF8.GetBytes(new string('A', 1024 * 1024)); // 1MB
            byte[] encrypted = null, decrypted = null;

            var sw = Stopwatch.StartNew();
            using (var encryptor = aes.CreateEncryptor())
            {
                encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);
            }
            sw.Stop();
            var encryptTime = sw.ElapsedMilliseconds;

            sw.Restart();
            using (var decryptor = aes.CreateDecryptor())
            {
                decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
            }
            sw.Stop();
            var decryptTime = sw.ElapsedMilliseconds;

            Log($"AES-{keySize} | Encrypt: {encryptTime} ms | Decrypt: {decryptTime} ms");
        }

        private static void TestRSA(int keySize)
        {
            var rsa = new RSACryptoServiceProvider(keySize);
            var data = Encoding.UTF8.GetBytes("Sample data to encrypt using RSA");
            byte[] encrypted = null, decrypted = null;

            var sw = Stopwatch.StartNew();
            encrypted = rsa.Encrypt(data, false);
            sw.Stop();
            var encryptTime = sw.ElapsedMilliseconds;

            sw.Restart();
            decrypted = rsa.Decrypt(encrypted, false);
            sw.Stop();
            var decryptTime = sw.ElapsedMilliseconds;

            Log($"RSA-{keySize} | Encrypt: {encryptTime} ms | Decrypt: {decryptTime} ms");
        }

        private static void Log(string message)
        {
            Console.WriteLine(message);
            File.AppendAllText(LogPath, message + "\n");
        }
    }
}
