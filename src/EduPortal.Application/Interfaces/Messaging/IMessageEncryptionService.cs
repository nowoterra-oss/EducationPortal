namespace EduPortal.Application.Interfaces.Messaging;

/// <summary>
/// Mesaj sifreleme servisi - AES-256 ve SHA-256
/// </summary>
public interface IMessageEncryptionService
{
    /// <summary>
    /// Mesaji sifreler
    /// </summary>
    /// <param name="plainText">Duz metin</param>
    /// <param name="conversationId">Konusma ID (her konusma icin farkli anahtar)</param>
    /// <returns>Sifreli metin ve hash</returns>
    (string encryptedContent, string contentHash) Encrypt(string plainText, int conversationId);

    /// <summary>
    /// Sifreli mesaji cozer
    /// </summary>
    /// <param name="encryptedContent">Sifreli metin</param>
    /// <param name="conversationId">Konusma ID</param>
    /// <returns>Duz metin</returns>
    string Decrypt(string encryptedContent, int conversationId);

    /// <summary>
    /// Mesajin butunlugunu dogrular (hash kontrolu)
    /// </summary>
    bool VerifyIntegrity(string plainText, string expectedHash);

    /// <summary>
    /// Konusma icin yeni sifreleme anahtari olusturur
    /// </summary>
    string GenerateConversationKey(int conversationId);

    /// <summary>
    /// SHA-256 hash hesaplar
    /// </summary>
    string ComputeHash(string content);
}
