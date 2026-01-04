using EduPortal.Application.Interfaces.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace EduPortal.Infrastructure.Services.Messaging;

/// <summary>
/// Mesaj sifreleme servisi - AES-256 ve SHA-256
/// </summary>
public class MessageEncryptionService : IMessageEncryptionService
{
    private readonly string _masterKey;
    private readonly Dictionary<int, byte[]> _conversationKeys = new();
    private readonly object _lockObject = new();
    private readonly ILogger<MessageEncryptionService> _logger;

    public MessageEncryptionService(IConfiguration configuration, ILogger<MessageEncryptionService> logger)
    {
        _logger = logger;

        // appsettings.json'dan master key al
        _masterKey = configuration["Messaging:EncryptionKey"]
            ?? "EduPortal_Default_Encryption_Key_2024_!@#$%^&*()";

        // Minimum 32 karakter olmali
        if (_masterKey.Length < 32)
        {
            _masterKey = _masterKey.PadRight(32, '_');
        }
    }

    public (string encryptedContent, string contentHash) Encrypt(string plainText, int conversationId)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return (string.Empty, string.Empty);
        }

        var key = GetOrCreateConversationKey(conversationId);
        var encrypted = EncryptAes(plainText, key);
        var hash = ComputeHash(plainText);

        return (encrypted, hash);
    }

    public string Decrypt(string encryptedContent, int conversationId)
    {
        if (string.IsNullOrEmpty(encryptedContent))
        {
            return string.Empty;
        }

        try
        {
            var key = GetOrCreateConversationKey(conversationId);
            return DecryptAes(encryptedContent, key);
        }
        catch (Exception ex)
        {
            // Sifre cozulemezse hatayi logla ve fallback degerini don
            _logger.LogError(ex, "[Decrypt] Failed to decrypt message for ConversationId={ConversationId}, EncryptedContentLength={Length}, Content={Content}",
                conversationId, encryptedContent?.Length ?? 0, encryptedContent?.Substring(0, Math.Min(50, encryptedContent?.Length ?? 0)));
            return "[Mesaj okunamadı]";
        }
    }

    public bool VerifyIntegrity(string plainText, string expectedHash)
    {
        if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(expectedHash))
        {
            return false;
        }

        var computedHash = ComputeHash(plainText);
        return computedHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
    }

    public string GenerateConversationKey(int conversationId)
    {
        // Yeni anahtar olustur ve cache'e ekle
        using var rng = RandomNumberGenerator.Create();
        var keyBytes = new byte[32]; // 256 bit
        rng.GetBytes(keyBytes);

        lock (_lockObject)
        {
            _conversationKeys[conversationId] = keyBytes;
        }

        return Convert.ToBase64String(keyBytes);
    }

    public string ComputeHash(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private byte[] GetOrCreateConversationKey(int conversationId)
    {
        lock (_lockObject)
        {
            if (_conversationKeys.TryGetValue(conversationId, out var existingKey))
            {
                return existingKey;
            }

            // Konusma icin deterministik anahtar olustur
            // Master key + conversation ID kombinasyonu
            var keySource = $"{_masterKey}_{conversationId}";
            using var sha256 = SHA256.Create();
            var key = sha256.ComputeHash(Encoding.UTF8.GetBytes(keySource));

            _conversationKeys[conversationId] = key;
            return key;
        }
    }

    private static string EncryptAes(string plainText, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // IV'i encrypted data'nin basina ekle
        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    private static string DecryptAes(string encryptedText, byte[] key)
    {
        var fullCipher = Convert.FromBase64String(encryptedText);

        using var aes = Aes.Create();
        aes.Key = key;

        // IV'i encrypted data'nin basindan al
        var iv = new byte[aes.BlockSize / 8];
        var cipher = new byte[fullCipher.Length - iv.Length];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
