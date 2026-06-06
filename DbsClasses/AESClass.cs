using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DbsClasses;

public sealed class AESClass
{
    private byte[]? _aesKey;
    public byte[] AESKey
    {
        get
        {
            if (_aesKey == null)
            {
                var filePath = ".env";
                if (File.Exists(filePath))
                {
                    _aesKey = File.ReadAllBytes(filePath);
                }
                else
                {
                    _aesKey = new byte[32];
                    RandomNumberGenerator.Create().GetBytes(_aesKey);
                    File.WriteAllBytes(filePath, _aesKey);
                }
            }
            return _aesKey;
        }
    }
    public string Encrypt(string plainText)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        
        byte[] nonce = new byte[12];
        byte[] keyyy = AESKey;
        byte[] ciphertext = new byte[plainBytes.Length];
        byte[] tag = new byte[16];
        RandomNumberGenerator.Fill(nonce);

        using (AesCcm aesCcm = new(keyyy))
        {
            aesCcm.Encrypt(nonce, plainBytes, ciphertext, tag, null);
        }

        byte[] result = new byte[nonce.Length + ciphertext.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(ciphertext, 0, result, nonce.Length, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length + ciphertext.Length, tag.Length);
        
        return Convert.ToBase64String(result);
    }
    public string Decrypt(string cipherTextBase64)
    {
        byte[] full = Convert.FromBase64String(cipherTextBase64);

        // Минимальная длина: nonce + tag (даже если ciphertext пустой)
        if (full.Length < 12 + 16)
            throw new ArgumentException("Invalid encrypted data: too short");

        // Извлекаем nonce, ciphertext, tag
        byte[] nonce = new byte[12];
        Buffer.BlockCopy(full, 0, nonce, 0, 12);

        int ciphertextLen = full.Length - 12 - 16;
        byte[] ciphertext = new byte[ciphertextLen];
        Buffer.BlockCopy(full, 12, ciphertext, 0, ciphertextLen);

        byte[] tag = new byte[16];
        Buffer.BlockCopy(full, 12 + ciphertextLen, tag, 0, 16);

        byte[] decryptedBytes = new byte[ciphertextLen];

        using (var aesCcm = new AesCcm(AESKey))
        {
            aesCcm.Decrypt(nonce, ciphertext, tag, decryptedBytes, null);
        }

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}