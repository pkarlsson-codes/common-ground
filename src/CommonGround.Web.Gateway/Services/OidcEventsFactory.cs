using System.Security.Claims;
using System.Security.Cryptography;
using CommonGround.Web.Gateway.Configuration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace CommonGround.Web.Gateway.Services;

/// <summary>
/// Factory for creating OpenID Connect event handlers used by the gateway.
/// </summary>
public sealed class OidcEventsFactory(
    ITokenStore tokenStore,
    IHostEnvironment env)
{
    /// <summary>
    /// Creates the <see cref="OpenIdConnectEvents"/> configured for the gateway.
    /// </summary>
    /// <returns>Configured OpenID Connect events.</returns>
    public OpenIdConnectEvents Create()
    {
        return new OpenIdConnectEvents
        {
            OnTokenValidated = HandleTokenValidatedAsync,
            OnRedirectToIdentityProviderForSignOut = context =>
            {
                context.ProtocolMessage.ClientId = context.Options.ClientId;
                return Task.CompletedTask;
            },
        };
    }

    private static bool TryGetSubject(ClaimsPrincipal? principal, out string subject)
    {
        subject = string.Empty;

        if (principal == null)
        {
            return false;
        }

        var subClaim = principal.FindFirst("sub")?.Value;
        if (!string.IsNullOrWhiteSpace(subClaim))
        {
            subject = subClaim;
            return true;
        }

        subClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(subClaim))
        {
            subject = subClaim;
            return true;
        }

        return false;
    }

    private static void AddUserIdClaim(TokenValidatedContext ctx, string userId)
    {
        var identity = (ClaimsIdentity)ctx.Principal!.Identity!;
        identity.AddClaim(new Claim("userId", userId));
    }

    private async Task HandleTokenValidatedAsync(TokenValidatedContext ctx)
    {
        if (!TryGetSubject(ctx.Principal, out var subject))
        {
            ctx.Fail("Missing subject (sub) claim from identity provider");
            return;
        }

        // In V1 the userId is the OIDC subject (Keycloak UUID).
        var userId = subject;
        AddUserIdClaim(ctx, userId);
        await PersistTokensAndIssueCookiesAsync(ctx, userId).ConfigureAwait(false);
    }

    private async Task PersistTokensAndIssueCookiesAsync(TokenValidatedContext ctx, string userId)
    {
        var accessToken = ctx.TokenEndpointResponse?.AccessToken;
        var refreshToken = ctx.TokenEndpointResponse?.RefreshToken;
        var idToken = ctx.TokenEndpointResponse?.IdToken;

        var sessionId = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        await tokenStore.SaveTokensAsync(sessionId, userId, accessToken, refreshToken, idToken, ctx.HttpContext.RequestAborted).ConfigureAwait(false);

        var isProd = env.IsProduction();

        ctx.HttpContext.Response.Cookies.Append(
            GatewayDefaults.Cookies.SessionCookie,
            sessionId,
            new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = isProd,
                Path = "/",
                MaxAge = TimeSpan.FromDays(7),
            });

        var csrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        ctx.HttpContext.Response.Cookies.Append(
            GatewayDefaults.Cookies.CsrfCookie,
            csrfToken,
            new CookieOptions
            {
                HttpOnly = false,
                SameSite = SameSiteMode.Strict,
                Secure = isProd,
                Path = "/",
                MaxAge = TimeSpan.FromDays(7),
            });
    }
}
