using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using CommonGround.SharedKernel.Domain;
using Microsoft.IdentityModel.Tokens;

namespace CommonGround.API.IntegrationTests;

/// <summary>
/// Mints signed JWTs for use in integration tests.
/// </summary>
public static class TestJwtFactory
{
    /// <summary>
    /// Creates a signed token that matches API validation settings.
    /// </summary>
    /// <param name="userId">The user identifier to include in the token.</param>
    /// <param name="roles">Optional role claims to include.</param>
    /// <param name="trustId">Optional trust identifier to include as a <c>tenant_id</c> claim.</param>
    /// <param name="additionalClaims">Optional additional claims to include.</param>
    /// <returns>A serialized JWT string.</returns>
    public static string CreateToken(
        UserId userId,
        string[]? roles = null,
        string? trustId = null,
        IEnumerable<Claim>? additionalClaims = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiFactory.JwtSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("user_id", userId.Value),
            new(JwtRegisteredClaimNames.Sub, userId.Value),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (roles is not null)
        {
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        }

        if (!string.IsNullOrEmpty(trustId))
        {
            claims.Add(new Claim("tenant_id", trustId));
        }

        if (additionalClaims is not null)
        {
            claims.AddRange(additionalClaims);
        }

        var token = new JwtSecurityToken(
            issuer: ApiFactory.JwtIssuer,
            audience: ApiFactory.JwtAudience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Adds a valid bearer token to the HTTP client for authenticated requests.
    /// </summary>
    /// <param name="client">The HTTP client to decorate.</param>
    /// <param name="userId">Optional explicit user ID value.</param>
    /// <param name="trustId">Optional trust identifier; defaults to <see cref="ApiFactory.TestTrustId"/>.</param>
    /// <returns>The same client for fluent chaining.</returns>
    public static HttpClient WithAuthToken(this HttpClient client, string? userId = null, string? trustId = null)
    {
        var actorId = userId is null ? UserId.Generate() : UserId.Create(userId);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateToken(actorId, trustId: trustId ?? ApiFactory.TestTrustId));
        return client;
    }

    /// <summary>
    /// Adds a service token that satisfies the ServiceToken authorization policy.
    /// </summary>
    /// <param name="client">The HTTP client to decorate.</param>
    /// <param name="userId">Optional explicit user ID value.</param>
    /// <returns>The same client for fluent chaining.</returns>
    public static HttpClient WithServiceToken(this HttpClient client, string? userId = null)
    {
        var actorId = userId is null ? UserId.Generate() : UserId.Create(userId);
        var claims = new[]
        {
            new Claim("actor", "gateway"),
        };

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateToken(actorId, trustId: null, additionalClaims: claims));

        return client;
    }
}
