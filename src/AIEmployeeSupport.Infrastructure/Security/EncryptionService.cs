using System.Security.Cryptography;
using System.Text;
using AIEmployeeSupport.Application.Common.Settings;
using AIEmployeeSupport.Application.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace AIEmployeeSupport.Infrastructure.Security;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IOptions<EncryptionSettings> encryptionSettings)
    {
        var keyString = encryptionSettings.Value.Key;
        if (string.IsNullOrEmpty(keyString))
        {
            throw new InvalidOperationException("Encryption key is not configured.");
        }

        // Derive a 256-bit (32-byte) key using SHA256 to ensure valid AES-256 key length 
        // regardless of the provided configuration key's original length.
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return plainText;
        }

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV(); // Automatically generates a cryptographically strong 16-byte IV

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        
        // Prepend IV to the stream so it can be extracted during decryption
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return cipherText;
        }

        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;

        // Extract IV from the first 16 bytes
        var iv = new byte[aes.BlockSize / 8];
        if (fullCipher.Length < iv.Length)
        {
            throw new CryptographicException("Invalid cipher text format.");
        }
        
        Array.Copy(fullCipher, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        
        return sr.ReadToEnd();
    }
}
