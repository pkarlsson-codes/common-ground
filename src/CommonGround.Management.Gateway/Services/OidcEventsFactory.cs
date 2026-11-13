using System.Security.Claims;
using System.Security.Cryptography;
using CommonGround.API.Clients;
using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.Management.Gateway.Configuration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace CommonGround.Management.Gateway.Services;

/// <summary>
/// Factory for creating OpenID Connect event handlers used by the gateway.
/// </summary>
public sealed class OidcEventsFactory(
    ITokenStore tokenStore,
    IHostEnvironment env,
    IUsersApiClient usersApiClient)
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

            // Keycloak opens its registration form for prompt=create only when it is in the query string,
            // not inside a pushed authorization request. So registration skips the push.
            OnPushAuthorization = context =>
            {
                if (context.ProtocolMessage.Prompt == "create")
                {
                    context.SkipPush();
                }

                return Task.CompletedTask;
            },
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

    private async Task HandleTokenValidatedAsync(TokenValidatedContext ctx)
    {
        if (!TryGetSubject(ctx.Principal, out var subject))
        {
            ctx.Fail("Missing subject (sub) claim from identity provider");
            return;
        }

        var email = ctx.Principal?.FindFirst("email")?.Value;

        // Not every provider puts "name" in the ID token (the dev realm's profile scope gives only the parts).
        var fullName = string.Join(' ', new[] { ctx.Principal?.FindFirst("given_name")?.Value, ctx.Principal?.FindFirst("family_name")?.Value }
            .Where(part => !string.IsNullOrWhiteSpace(part)));
        var displayName = ctx.Principal?.FindFirst("name")?.Value
                       ?? (fullName.Length > 0 ? fullName : null)
                       ?? ctx.Principal?.FindFirst("preferred_username")?.Value;

        ctx.Properties!.Items.TryGetValue("trust_id", out var trustId);
        trustId = string.IsNullOrWhiteSpace(trustId) ? null : trustId;

        var response = await usersApiClient.EnsureUserAsync(
            new EnsureUserRequest(subject, email, displayName, trustId),
            ctx.HttpContext.RequestAborted).ConfigureAwait(false);

        var userId = response.UserId;
        await PersistTokensAndIssueCookiesAsync(ctx, userId, response.TrustId).ConfigureAwait(false);
    }

    private async Task PersistTokensAndIssueCookiesAsync(TokenValidatedContext ctx, string userId, string? trustId)
    {
        var accessToken = ctx.TokenEndpointResponse?.AccessToken;
        var refreshToken = ctx.TokenEndpointResponse?.RefreshToken;
        var idToken = ctx.TokenEndpointResponse?.IdToken;

        var sessionId = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        await tokenStore.SaveTokensAsync(sessionId, userId, trustId, accessToken, refreshToken, idToken, ctx.HttpContext.RequestAborted).ConfigureAwait(false);

        var isProd = env.IsProduction();

        ctx.HttpContext.Response.Cookies.Append(
            GatewayDefaults.Cookies.SessionCookie,
            sessionId,
            new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
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
                SameSite = SameSiteMode.Lax,
                Secure = isProd,
                Path = "/",
                MaxAge = TimeSpan.FromDays(7),
            });
    }
}
