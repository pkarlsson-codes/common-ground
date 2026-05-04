using CommonGround.SharedKernel.Domain;

namespace CommonGround.API.Infrastructure.Auth;

/// <summary>
/// Provides extension methods for retrieving user identity information from the HTTP context.
/// </summary>
internal static class HttpContextIdentity
{
    private const string UserIdClaimType = "user_id";

    /// <summary>
    /// Retrieves the user ID from the HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context containing the user claims.</param>
    /// <returns>The user ID extracted from the HTTP context.</returns>
    public static UserId GetUserId(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var id = httpContext.User?.FindFirst(UserIdClaimType)?.Value
            ?? throw new InvalidOperationException($"Claim '{UserIdClaimType}' not found. Ensure the endpoint is authenticated.");

        return UserId.Create(id);
    }

    /// <summary>
    /// Retrieves the tenant ID string from the HTTP context's user claims.
    /// </summary>
    /// <param name="httpContext">The HTTP context containing the user claims.</param>
    /// <returns>The tenant ID string extracted from the claims.</returns>
    public static string GetTenantId(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        return httpContext.User?.FindFirst("tenant_id")?.Value
            ?? throw new InvalidOperationException("Claim 'tenant_id' not found. Ensure the endpoint is authenticated.");
    }

    /// <summary>
    /// Attempts to retrieve the tenant ID from the HTTP context's user claims. Returns null if not found or if the claim value is invalid.
    /// </summary>
    /// <param name="httpContext">The HTTP context containing the user claims.</param>
    /// <returns>The tenant ID if available; otherwise, null.</returns>
    public static TrustId? TryGetTenantId(this HttpContext httpContext)
    {
        var id = httpContext.User?.FindFirst("tenant_id")?.Value;
        return id is not null ? TrustId.Create(id) : null;
    }
}
