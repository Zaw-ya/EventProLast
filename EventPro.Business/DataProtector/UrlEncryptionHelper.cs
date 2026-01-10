using System.Security.Cryptography;
using System.Text;

namespace EventPro.Business.DataProtector
{
    public static class UrlEncryptionHelper
    {
        private static readonly byte[] Key = Convert.FromBase64String("cZx2nQqjX7a2Pj3QFq8yKkD6w9mZ7r5YtXhVj1LmO6Q=");

        private const int NonceSize = 12; // recommended for AES-GCM
        private const int TagSize = 16;

        public static string Encrypt(string plainText)
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            var nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            var cipher = new byte[plaintextBytes.Length];
            var tag = new byte[TagSize];

            using (var aes = new AesGcm(Key))
            {
                aes.Encrypt(nonce, plaintextBytes, cipher, tag);
            }

            // Compose: nonce || tag || cipher
            var outBytes = new byte[nonce.Length + tag.Length + cipher.Length];
            Buffer.BlockCopy(nonce, 0, outBytes, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, outBytes, nonce.Length, tag.Length);
            Buffer.BlockCopy(cipher, 0, outBytes, nonce.Length + tag.Length, cipher.Length);

            return Base64UrlEncode(outBytes);
        }

        public static string Decrypt(string cipherText)
        {
            var all = Base64UrlDecode(cipherText);
            var nonce = new byte[NonceSize];
            var tag = new byte[TagSize];
            var cipher = new byte[all.Length - NonceSize - TagSize];

            Buffer.BlockCopy(all, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(all, NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(all, NonceSize + TagSize, cipher, 0, cipher.Length);

            var plain = new byte[cipher.Length];
            using (var aes = new AesGcm(Key))
            {
                aes.Decrypt(nonce, cipher, tag, plain);
            }

            return Encoding.UTF8.GetString(plain);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            // Standard base64 then make it URL-safe
            return Convert.ToBase64String(input)
                .TrimEnd('=')      // remove padding chars
                .Replace('+', '-') // 62nd char of encoding
                .Replace('/', '_'); // 63rd char of encoding
        }

        private static byte[] Base64UrlDecode(string input)
        {
            string s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }
    }
}
