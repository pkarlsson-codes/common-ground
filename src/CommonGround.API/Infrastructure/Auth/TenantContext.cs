using CommonGround.SharedKernel.Domain;

namespace CommonGround.API.Infrastructure.Auth;

/// <summary>
/// Tenant context implementation that retrieves tenant information from the HTTP context using the "tenant_id" claim. This allows services to access tenant-specific data based on the authenticated user's claims.
/// </summary>
/// <param name="httpContextAccessor">The HTTP context accessor used to access the current HTTP context.</param>
internal sealed class TenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    /// <summary>
    /// Gets the tenant's trust ID from the HTTP context's user claims. Returns null if the claim is not present or if the HTTP context is not available.
    /// </summary>
    public TrustId? TrustId { get; } =
        httpContextAccessor.HttpContext?.TryGetTenantId();

    /// <summary>
    /// Indicates whether the current context has tenant information available, which is determined by the presence of a valid tenant ID in the HTTP context's user claims.
    /// </summary>
    public bool HasTenant => TrustId is not null;
}
