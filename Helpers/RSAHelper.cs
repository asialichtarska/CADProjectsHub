using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace CADProjectsHub.Helpers
{
    public class RSAHelper
    {
        public void AssignNewKey(string publicKeyPath, string privateKeyPath)
        {
            using (var rsa = new RSACryptoServiceProvider(2048)) // Użycie RSA 2048-bit
            {
                rsa.PersistKeyInCsp = false;

                File.WriteAllText(publicKeyPath, rsa.ToXmlString(false));
                File.WriteAllText(privateKeyPath, rsa.ToXmlString(true));
            }
        }

        //Hybrydowe szyfrowanie dużego pliku (AES + RSA)
        public byte[] EncryptFile(byte[] fileData, string publicKey)
        {
            using var aes = Aes.Create();
            aes.GenerateKey();
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var encryptedFileData = encryptor.TransformFinalBlock(fileData, 0, fileData.Length);

            using var rsa = new RSACryptoServiceProvider(2048);
            rsa.PersistKeyInCsp = false;
            rsa.FromXmlString(publicKey);

            var encryptedKey = rsa.Encrypt(aes.Key, false);
            var encryptedIV = rsa.Encrypt(aes.IV, false);

            using var ms = new MemoryStream();
            ms.Write(encryptedKey, 0, encryptedKey.Length);      // 256B
            ms.Write(encryptedIV, 0, encryptedIV.Length);        // 256B
            ms.Write(encryptedFileData, 0, encryptedFileData.Length);

            return ms.ToArray();
        }

        //Hybrydowe deszyfrowanie dużego pliku
        public byte[] DecryptFile(byte[] encryptedData, string privateKey)
        {
            using var rsa = new RSACryptoServiceProvider(2048);
            rsa.PersistKeyInCsp = false;
            rsa.FromXmlString(privateKey);

            byte[] encryptedKey = encryptedData.Take(256).ToArray();
            byte[] encryptedIV = encryptedData.Skip(256).Take(256).ToArray();
            byte[] encryptedFileData = encryptedData.Skip(512).ToArray();

            byte[] aesKey = rsa.Decrypt(encryptedKey, false);
            byte[] aesIV = rsa.Decrypt(encryptedIV, false);

            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = aesIV;

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(encryptedFileData, 0, encryptedFileData.Length);
        }

        //Generowanie SHA-256 dla danych
        public string GenerateChecksum(byte[] fileData)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(fileData);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        //Tworzenie podpisu cyfrowego sumy SHA256
        public byte[] SignData(string checksum, string privateKey)
        {
            using var rsa = new RSACryptoServiceProvider(2048);
            rsa.PersistKeyInCsp = false;
            rsa.FromXmlString(privateKey);

            var dataToSign = Encoding.UTF8.GetBytes(checksum);
            return rsa.SignData(dataToSign, CryptoConfig.MapNameToOID("SHA256"));
        }

        //Weryfikacja podpisu SHA256
        public bool VerifySignature(string checksum, byte[] signature, string publicKey)
        {
            using var rsa = new RSACryptoServiceProvider(2048);
            rsa.PersistKeyInCsp = false;
            rsa.FromXmlString(publicKey);

            var dataToVerify = Encoding.UTF8.GetBytes(checksum);
            return rsa.VerifyData(dataToVerify, CryptoConfig.MapNameToOID("SHA256"), signature);
        }
    }
}
