using CommonGround.Management.Gateway.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace CommonGround.Management.Gateway.Configuration;

/// <summary>
/// Authentication registration helpers for cookie + OpenID Connect.
/// </summary>
public static class AuthenticationServiceCollectionExtensions
{
    /// <summary>
    /// Registers cookie auth and OIDC (Keycloak) with gateway-specific defaults.
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCookieAndOidcAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = GatewayDefaults.Cookies.AuthenticationCookie;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
            })
            .AddOpenIdConnect(options =>
            {
                options.Authority = configuration["Authentication:OIDC:Authority"];

                // Optional internal address of the discovery document, for when the gateway can't reach the
                // provider at its public URL (Keycloak behind the same proxy, with backchannel URLs dynamic).
                var metadataAddress = configuration["Authentication:OIDC:MetadataAddress"];
                if (!string.IsNullOrWhiteSpace(metadataAddress))
                {
                    options.MetadataAddress = metadataAddress;
                }

                options.ClientId = configuration["Authentication:OIDC:ClientId"];
                options.ClientSecret = configuration["Authentication:OIDC:ClientSecret"];
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.UsePkce = true;
                options.SaveTokens = false;
                options.CallbackPath = "/auth/callback";
                options.GetClaimsFromUserInfoEndpoint = true;

                // Keep the provider's claim names ("email", "name", "sub"): the gateway reads them by those names.
                // Mapped, "email" would arrive as ClaimTypes.Email and users would be registered without one.
                options.MapInboundClaims = false;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.TokenValidationParameters.NameClaimType = "preferred_username";
                options.TokenValidationParameters.RoleClaimType = "roles";
                options.RequireHttpsMetadata = false;
                options.SignedOutCallbackPath = "/auth/signedout-callback";
                options.SignedOutRedirectUri = configuration["Authentication:OIDC:PostLogoutRedirect"]
                    ?? configuration["Authentication:OIDC:PostLoginRedirect"]
                    ?? "/";
            });

        services.AddSingleton<OidcEventsFactory>();
        services.AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
            .Configure<OidcEventsFactory>((options, factory) => options.Events = factory.Create());

        services.AddHttpClient();
        return services;
    }
}
