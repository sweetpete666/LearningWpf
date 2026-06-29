using System.Security.Cryptography;
using System.Text;

namespace Company.Shared.Bootstrapping
{
    public static class CryptoHelper
    {
        // 1. Dein geheimer AES-Schlüssel (Muss exakt 32 Bytes für AES-256 lang sein)
        // WICHTIG: Ändere diese Zahlen vor dem Release in deine eigenen geheimen Zahlen (0-255)!
        private static readonly byte[] AesKey = new byte[32]
        {
            12, 45, 78, 111, 144, 177, 200, 233,
            55, 66, 77,  88,  99, 110, 122, 133,
            14, 25, 36,  47,  58,  69,  80,  91,
            2,  4,  6,   8,  10,  12,  14,  16
        };

        // 2. Der Initialisierungsvektor (Muss exakt 16 Bytes lang sein)
        private static readonly byte[] AesIv = new byte[16]
        {
            9, 87, 65, 43, 21, 12, 34, 56,
            78, 90, 11, 22, 33, 44, 55, 66
        };

        /// <summary>
        /// Verschlüsselt einen Klartext-String in einen sicheren Base64-String.
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            using var aes = Aes.Create();
            aes.Key = AesKey;
            aes.IV = AesIv;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs, Encoding.UTF8))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// Entschlüsselt einen Base64-AES-String zurück in den Klartext.
        /// Wirft eine Exception, wenn der String kein gültiges AES ist.
        /// </summary>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            using var aes = Aes.Create();
            aes.Key = AesKey;
            aes.IV = AesIv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);

            return sr.ReadToEnd();
        }
    }
}
