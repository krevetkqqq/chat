using System.Security.Cryptography;
using System.Text;

namespace DbsClasses;

public sealed class RSAClass
{
    public RSAClass() // for init
    {
        if (File.Exists("rsa.env")) return;
        using RSA rsa = RSA.Create(2048);
        byte[] publicKeyBytes = rsa.ExportRSAPublicKey();
        byte[] privateKeyBytes = rsa.ExportRSAPrivateKey();
        _rsaKey = privateKeyBytes;
        File.WriteAllBytes("rsa.env", privateKeyBytes);
        File.WriteAllBytes("rsapub.env", publicKeyBytes);
    }
    private byte[]? _rsaKey;
    public byte[] RSAKey
    {
        get
        {
            if (_rsaKey == null)
            {
                if (File.Exists("rsa.env"))
                {
                    _rsaKey = File.ReadAllBytes("rsa.env");
                }
                else
                {
                    using RSA rsa = RSA.Create(2048);
                    byte[] publicKeyBytes = rsa.ExportRSAPublicKey();
                    byte[] privateKeyBytes = rsa.ExportRSAPrivateKey();
                    _rsaKey = privateKeyBytes;
                    File.WriteAllBytes("rsa.env", privateKeyBytes);
                    File.WriteAllBytes("rsapub.env", publicKeyBytes);
                }
            }
            return _rsaKey;
        }
    }
    public static byte[] GetPublicKey()
    {
        return File.ReadAllBytes("rsapub.env");
    }
    public string Decrypt(string cipherTextBase64)
    {
        using RSA rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(RSAKey, out _);
        
        byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(cipherTextBase64), RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(decryptedData);
    }
}