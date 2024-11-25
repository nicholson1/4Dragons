using System;
using System.Security.Cryptography;

namespace _Scripts.PlayFab
{

public static class EncryptionUtility
{
    
    private static readonly byte[] Key = new byte[32] 
    {
        0xf6, 0xa5, 0x45, 0xc1, 0xc8, 0x45, 0x7f, 0x4d,
        0xb0, 0xbc, 0xaa, 0x78, 0x0e, 0x46, 0x10, 0xcc,
        0x51, 0x99, 0x62, 0x26, 0x90, 0x90, 0x1a, 0x5c,
        0x6e, 0x32, 0xa8, 0xaa, 0xae, 0x71, 0x5d, 0x69
    };

    private static readonly byte[] IV = new byte[16]
    {
        0x2d, 0x8e, 0x3f, 0xd3, 0x48, 0x62, 0x23, 0xfa,
        0x41, 0x40, 0x34, 0x71, 0x05, 0xa7, 0xd1, 0xfb
    };

    // Encrypt data using AES algorithm
    public static string Encrypt(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            byte[] encryptedBytes = null;

            using (System.IO.MemoryStream msEncrypt = new System.IO.MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (System.IO.StreamWriter swEncrypt = new System.IO.StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    encryptedBytes = msEncrypt.ToArray();
                }
            }

            return Convert.ToBase64String(encryptedBytes);
        }
    }

    // Decrypt data using AES algorithm
    public static string Decrypt(string cipherText)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            string plaintext = null;

            using (System.IO.MemoryStream msDecrypt = new System.IO.MemoryStream(cipherBytes))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (System.IO.StreamReader srDecrypt = new System.IO.StreamReader(csDecrypt))
                    {
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }

            return plaintext;
        }
    }
}

}