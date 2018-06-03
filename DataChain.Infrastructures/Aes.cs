using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DataChain.Infrastructure
{
    public class Aes256
    {

        private static byte[] saltValueBytes;
        private static byte[] initVectorBytes;

        public static string RandomKey { get; private set; }

        static Aes256()
        {
            using (RandomNumberGenerator rand = new RNGCryptoServiceProvider())
            {
                saltValueBytes = new byte[16];
                rand.GetNonZeroBytes(saltValueBytes);
                initVectorBytes = new byte[16];
                rand.GetNonZeroBytes(initVectorBytes);
            }

        }


        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static byte[] GetBytes(string randomKeyText)
        {
            byte[] bytes = new byte[randomKeyText.Length * sizeof(char)];
            Buffer.BlockCopy(randomKeyText.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static byte[] Encode(byte[] byteValue)
        {

            var password = new PasswordDeriveBytes(GetUniqueString(8), saltValueBytes, "SHA1", 2);
            byte[] keyBytes = password.GetBytes(32);
            RandomKey = GetString(keyBytes);
            byte[] cipherBytes;

            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
           
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(
                                                             keyBytes,
                                                             initVectorBytes);

            using (MemoryStream memStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(byteValue, 0, byteValue.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherBytes = memStream.ToArray();

                }
            }

            return cipherBytes;

        }

        public static byte[] Decode(byte[] byteValue, string randomKey)
        {

            RijndaelManaged symmetricKey = new RijndaelManaged();
            
            symmetricKey.Mode = CipherMode.CBC;

            byte[] keyBytes = GetBytes(randomKey);
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(
                                                             keyBytes,
                                                             initVectorBytes);
            byte[] valueBytes;

            using (MemoryStream memStream = new MemoryStream(byteValue))
            {
                using (CryptoStream cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read))
                {
                    valueBytes = new byte[byteValue.Length];

                    int decryptedByteCount = cryptoStream.Read(
                                                            valueBytes,
                                                            0,
                                                            valueBytes.Length);
                }
            }

            return valueBytes;
        }

        public static string GetUniqueString(int maxSize)
        {

            char[] chars = new char[62];
            chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];

            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {

                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }

            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
    }
}
