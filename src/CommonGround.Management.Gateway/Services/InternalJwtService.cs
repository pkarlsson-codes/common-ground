using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CommonGround.Management.Gateway.Services;

/// <summary>
/// Provides internal JWT generation for gateway-to-API communication.
/// </summary>
public interface IInternalJwtService
{
    /// <summary>
    /// Generates a short-lived internal JWT for a given user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="trustId">Optional trust (tenant) identifier.</param>
    /// <param name="sessionId">Optional session identifier.</param>
    /// <returns>Signed JWT string.</returns>
    string GenerateToken(string? userId, string? trustId, string? sessionId);

    /// <summary>
    /// Generates a short-lived service token used by the BFF to call downstream services.
    /// </summary>
    /// <param name="sessionId">Optional session identifier to correlate calls.</param>
    /// <returns>Signed JWT string.</returns>
    string GenerateServiceToken(string? sessionId = null);
}

/// <summary>
/// Implementation of <see cref="IInternalJwtService"/>.
/// </summary>
public sealed class InternalJwtService : IInternalJwtService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly SigningCredentials _credentials;
    private readonly TimeSpan _lifetime;

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalJwtService"/> class.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="environment">The host environment.</param>
    public InternalJwtService(IConfiguration configuration, IHostEnvironment environment)
    {
        _issuer = configuration["InternalJwt:Issuer"] ?? "CommonGround.Gateway";
        _audience = configuration["InternalJwt:Audience"] ?? "CommonGround.API";

        var secret = configuration["InternalJwt:SigningKey"]
                     ?? Environment.GetEnvironmentVariable("COMMONGROUND_INTERNAL_JWT_SECRET")
                     ?? (environment.IsDevelopment() ? "dev-internal-jwt-secret-change-me" : null);

        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("Internal JWT signing key is not configured.");
        }

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        _credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        _lifetime = TimeSpan.FromMinutes(5);
    }

    /// <inheritdoc/>
    public string GenerateToken(string? userId, string? trustId, string? sessionId)
    {
        var now = DateTimeOffset.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Iss, _issuer),
            new(JwtRegisteredClaimNames.Aud, _audience),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Nbf, now.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Exp, now.Add(_lifetime).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };

        var subject = userId ?? "anonymous";
        claims.Add(new Claim(JwtRegisteredClaimNames.Sub, subject));

        if (!string.IsNullOrWhiteSpace(userId))
        {
            claims.Add(new Claim("user_id", userId));
        }

        if (!string.IsNullOrWhiteSpace(trustId))
        {
            claims.Add(new Claim("tenant_id", trustId));
        }

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            claims.Add(new Claim("session_id", sessionId));
        }

        var jwt = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.Add(_lifetime).UtcDateTime,
            signingCredentials: _credentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    /// <inheritdoc/>
    public string GenerateServiceToken(string? sessionId = null)
    {
        var now = DateTimeOffset.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Iss, _issuer),
            new(JwtRegisteredClaimNames.Aud, _audience),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Nbf, now.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Exp, now.Add(_lifetime).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Sub, "gateway-service"),
            new("actor", "gateway"),
        };

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            claims.Add(new Claim("session_id", sessionId));
        }

        var jwt = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.Add(_lifetime).UtcDateTime,
            signingCredentials: _credentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
