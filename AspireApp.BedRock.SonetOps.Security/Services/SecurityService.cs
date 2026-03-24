using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace AspireApp.BedRock.SonetOps.Security.Services;

public interface ISecurityService
{
    Task<string> GenerateJwtTokenAsync(string id, string role, Dictionary<string, string> claims);
    Task<bool> ValidateJwtTokenAsync(string token);
    Task<bool> ValidateApiKeyAsync(string apiKey, string requiredRole);
    Task<string> GenerateApiKeyAsync(string role);
    Task<string> EncryptMessageAsync(string message, string recipientPublicKey);
    Task<string> DecryptMessageAsync(string encryptedMessage);
    Task<bool> VerifySignatureAsync(string message, string signature, string publicKey);
    Task<string> SignMessageAsync(string message);
    Task<string> GenerateSharedSecretAsync(string peerId);
}

public class SecurityService : ISecurityService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<SecurityService> _logger;
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly ECDiffieHellman _keyExchangeKey;
    private readonly Dictionary<string, byte[]> _sharedSecrets = new();
    private readonly RSA _signingKey;

    public SecurityService(
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<SecurityService> logger)
    {
        _cache = cache;
        _logger = logger;
        _jwtKey = configuration["Security:JwtKey"] ?? throw new ArgumentNullException("Security:JwtKey");
        _jwtIssuer = configuration["Security:JwtIssuer"] ?? throw new ArgumentNullException("Security:JwtIssuer");
        
        // Initialize cryptographic keys
        _keyExchangeKey = ECDiffieHellman.Create();
        _signingKey = RSA.Create(2048);
    }

    public async Task<string> GenerateJwtTokenAsync(string id, string role, Dictionary<string, string> claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtKey);

        var claimsList = new List<Claim>
        {
            new Claim(ClaimTypes.Name, id),
            new Claim(ClaimTypes.Role, role)
        };

        claimsList.AddRange(claims.Select(c => new Claim(c.Key, c.Value)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claimsList),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _jwtIssuer,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<bool> ValidateJwtTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ValidateApiKeyAsync(string apiKey, string requiredRole)
    {
        var cacheKey = $"apikey_{apiKey}";
        
        if (!_cache.TryGetValue<ApiKeyInfo>(cacheKey, out var keyInfo))
            return false;

        if (keyInfo.ExpiresAt < DateTime.UtcNow)
        {
            _cache.Remove(cacheKey);
            return false;
        }

        return keyInfo.Role == requiredRole;
    }

    public async Task<string> GenerateApiKeyAsync(string role)
    {
        var apiKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        
        _cache.Set($"apikey_{apiKey}", new ApiKeyInfo
        {
            Role = role,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });

        return apiKey;
    }

    public async Task<string> EncryptMessageAsync(string message, string recipientPublicKey)
    {
        var sharedSecret = await GetOrCreateSharedSecretAsync(recipientPublicKey);
        
        using var aes = Aes.Create();
        aes.Key = sharedSecret;
        
        var iv = aes.IV;
        var encryptedData = await EncryptWithAesAsync(message, aes);
        
        var result = new byte[iv.Length + encryptedData.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encryptedData, 0, result, iv.Length, encryptedData.Length);
        
        return Convert.ToBase64String(result);
    }

    public async Task<string> DecryptMessageAsync(string encryptedMessage)
    {
        var fullData = Convert.FromBase64String(encryptedMessage);
        
        var iv = new byte[16];
        var encryptedData = new byte[fullData.Length - 16];
        
        Buffer.BlockCopy(fullData, 0, iv, 0, 16);
        Buffer.BlockCopy(fullData, 16, encryptedData, 0, encryptedData.Length);

        using var aes = Aes.Create();
        aes.Key = await GetLatestSharedSecretAsync();
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(encryptedData);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);
        
        return await reader.ReadToEndAsync();
    }

    public async Task<bool> VerifySignatureAsync(string message, string signature, string publicKey)
    {
        var key = RSA.Create();
        key.ImportFromPem(publicKey);

        var data = Encoding.UTF8.GetBytes(message);
        var signatureBytes = Convert.FromBase64String(signature);

        return key.VerifyData(
            data,
            signatureBytes,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
    }

    public async Task<string> SignMessageAsync(string message)
    {
        var data = Encoding.UTF8.GetBytes(message);
        
        var signature = _signingKey.SignData(
            data,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
            
        return Convert.ToBase64String(signature);
    }

    public async Task<string> GenerateSharedSecretAsync(string peerId)
    {
        // Create new ephemeral key pair for this peer
        using var ephemeralKey = ECDiffieHellman.Create();
        var publicKey = ephemeralKey.PublicKey.ExportSubjectPublicKeyInfo();
        
        // Store the private key for later use
        _cache.Set($"dh_key_{peerId}", ephemeralKey.ExportParameters(true));
        
        return Convert.ToBase64String(publicKey);
    }

    private async Task<byte[]> GetOrCreateSharedSecretAsync(string peerPublicKey)
    {
        var publicKeyBytes = Convert.FromBase64String(peerPublicKey);
        using var peerKey = ECDiffieHellman.Create();
        peerKey.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

        var sharedSecret = _keyExchangeKey.DeriveKeyMaterial(peerKey.PublicKey);
        return sharedSecret;
    }

    private async Task<byte[]> GetLatestSharedSecretAsync()
    {
        // In a real implementation, this would manage multiple shared secrets
        // and handle key rotation
        return _sharedSecrets.Values.Last();
    }

    private async Task<byte[]> EncryptWithAesAsync(string data, Aes aes)
    {
        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            await sw.WriteAsync(data);
        }
        return ms.ToArray();
    }

    private class ApiKeyInfo
    {
        public string Role { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}