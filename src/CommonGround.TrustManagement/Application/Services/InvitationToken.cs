using System.Security.Cryptography;
using System.Text;

namespace CommonGround.TrustManagement.Application.Services;

/// <summary>The secret in an invite link, and the hash that is stored in its place.</summary>
public static class InvitationToken
{
    /// <summary>Makes a new random 256-bit token, URL-safe.</summary>
    public static string Generate() => Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    /// <summary>Hashes a token for storage and lookup: SHA-256 as lowercase hex.</summary>
    public static string Hash(string token)
    {
        ArgumentNullException.ThrowIfNull(token);
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
