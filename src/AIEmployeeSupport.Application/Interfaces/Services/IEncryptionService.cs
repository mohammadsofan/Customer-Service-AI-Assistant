namespace AIEmployeeSupport.Application.Interfaces.Services;

/// <summary>
/// Service for encrypting and decrypting sensitive data (e.g., API keys).
/// </summary>
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
