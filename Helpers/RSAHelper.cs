using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace CADProjectsHub.Helpers
{
    public class RSAHelper
    {
        private readonly CryptoSettings _cryptoSettings;
        private static readonly string LogPath = Path.Combine("wwwroot", "logs", "data_benchmark_log.txt");

        public RSAHelper(IOptions<CryptoSettings> cryptoSettings)
        {
            _cryptoSettings = cryptoSettings.Value;
        }

        public (string publicKey, string privateKey) GenerateKeys()
        {
            using var rsa = new RSACryptoServiceProvider(_cryptoSettings.RSAKeySize);
            return (rsa.ToXmlString(false), rsa.ToXmlString(true));
        }

        // =============================
        // HYBRYDOWE SZYFROWANIE PLIKU
        // =============================
        public byte[] EncryptFile(byte[] fileData, string publicKey)
        {
            var stopwatch = Stopwatch.StartNew();

            using var aes = Aes.Create();
            aes.KeySize = _cryptoSettings.AESKeySize;
            aes.GenerateKey();
            aes.GenerateIV();

            byte[] encryptedFile;
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cryptoStream.Write(fileData, 0, fileData.Length);
                cryptoStream.FlushFinalBlock();
                encryptedFile = ms.ToArray();
            }

            using var rsa = new RSACryptoServiceProvider(_cryptoSettings.RSAKeySize);
            rsa.PersistKeyInCsp = false;
            rsa.FromXmlString(publicKey);
            var encryptedKey = rsa.Encrypt(aes.Key, false);
            var encryptedIV = rsa.Encrypt(aes.IV, false);

            using var finalStream = new MemoryStream();
            using (var bw = new BinaryWriter(finalStream))
            {
                bw.Write(encryptedKey.Length);
                bw.Write(encryptedIV.Length);
                bw.Write(encryptedKey);
                bw.Write(encryptedIV);
                bw.Write(encryptedFile);
            }

            stopwatch.Stop();
            LogOperation("EncryptFile", "RSA/AES", aes.KeySize, _cryptoSettings.RSAKeySize, "FileSize", fileData.Length, stopwatch.ElapsedMilliseconds);

            return finalStream.ToArray();
        }

        // =============================
        // HYBRYDOWE ODSZYFROWANIE PLIKU
        // =============================
        public byte[] DecryptFile(byte[] encryptedData, string privateKey)
        {
            var stopwatch = Stopwatch.StartNew();

            using var rsa = new RSACryptoServiceProvider(_cryptoSettings.RSAKeySize);
            rsa.PersistKeyInCsp = false;
            rsa.FromXmlString(privateKey);

            using var ms = new MemoryStream(encryptedData);
            using var br = new BinaryReader(ms);

            int lenKey = br.ReadInt32();
            int lenIV = br.ReadInt32();
            var encryptedKey = br.ReadBytes(lenKey);
            var encryptedIV = br.ReadBytes(lenIV);
            var encryptedFile = br.ReadBytes((int)(ms.Length - ms.Position));

            var aesKey = rsa.Decrypt(encryptedKey, false);
            var aesIV = rsa.Decrypt(encryptedIV, false);

            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = aesIV;

            using var decryptor = aes.CreateDecryptor();
            using var inputStream = new MemoryStream(encryptedFile);
            using var cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read);
            using var resultStream = new MemoryStream();
            cryptoStream.CopyTo(resultStream);

            stopwatch.Stop();
            LogOperation("DecryptFile", "RSA/AES", aes.KeySize, _cryptoSettings.RSAKeySize, "FileSize", encryptedData.Length, stopwatch.ElapsedMilliseconds);

            return resultStream.ToArray();
        }

        // =============================
        // SZYFROWANIE / DESZYFROWANIE STRING (AES)
        // =============================
        public string EncryptStringAES(string plainText, byte[] key, byte[] iv)
        {
            var stopwatch = Stopwatch.StartNew();

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            stopwatch.Stop();
            LogOperation("EncryptStringAES", "AES", key.Length * 8, null, "DataSize", Encoding.UTF8.GetByteCount(plainText), stopwatch.ElapsedMilliseconds);

            return Convert.ToBase64String(ms.ToArray());
        }

        public string DecryptStringAES(string cipherTextBase64, byte[] key, byte[] iv)
        {
            var stopwatch = Stopwatch.StartNew();

            var cipherBytes = Convert.FromBase64String(cipherTextBase64);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            string decryptedText = sr.ReadToEnd();

            stopwatch.Stop();
            LogOperation("DecryptStringAES", "AES", key.Length * 8, null, "DataSize", cipherBytes.Length, stopwatch.ElapsedMilliseconds);

            return decryptedText;
        }

        private void LogOperation(string operation, string algorithm, int aesKeyBits, int? rsaKeyBits, string sizeType, int dataSizeBytes, long timeMs)
        {
            try
            {
                var logDir = Path.GetDirectoryName(LogPath);
                if (!string.IsNullOrEmpty(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                var rsaInfo = rsaKeyBits.HasValue ? $" | RSAKey: {rsaKeyBits.Value}" : string.Empty;
                File.AppendAllText(LogPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {operation} | {algorithm} | AESKey: {aesKeyBits}{rsaInfo} | {sizeType}: {dataSizeBytes} B | Time: {timeMs} ms{Environment.NewLine}");
            }
            catch
            {
                // Ignoruj błędy logowania
            }
        }

        public void RunAllTests()
        {
            // Szyfrowanie pliku testowego
            var filePath = Path.Combine("wwwroot", "uploads", "Microscope clamp.STEP.enc");
            var publicKeyPath = Path.Combine("wwwroot", "keys", "publicKey.xml");
            var privateKeyPath = Path.Combine("wwwroot", "keys", "privateKey.xml");

            if (File.Exists(filePath) && File.Exists(publicKeyPath) && File.Exists(privateKeyPath))
            {
                var encryptedFileData = File.ReadAllBytes(filePath);
                var privateKey = File.ReadAllText(privateKeyPath);
                _ = DecryptFile(encryptedFileData, privateKey);
            }

            // Szyfrowanie tekstu
            using var aes = Aes.Create();
            aes.GenerateKey();
            aes.GenerateIV();

            string constructorName = "John Doe";
            var encryptedString = EncryptStringAES(constructorName, aes.Key, aes.IV);
            _ = DecryptStringAES(encryptedString, aes.Key, aes.IV);
        }
    }
}
