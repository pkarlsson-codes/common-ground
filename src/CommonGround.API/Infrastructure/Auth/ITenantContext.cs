using CommonGround.SharedKernel.Domain;

namespace CommonGround.API.Infrastructure.Auth;

/// <summary>Context for tenant information.</summary>
public interface ITenantContext
{
    /// <summary>The tenant's trust ID, if available.</summary>
    TrustId? TrustId { get; }

    /// <summary>Indicates whether the current context has tenant information available.</summary>
    bool HasTenant { get; }
}

/// <summary>Extension methods for <see cref="ITenantContext"/>.</summary>
public static class TenantContextExtensions
{
    /// <summary>Gets the required tenant trust ID from the context, throwing an exception if it is not available.</summary>
    /// <param name="ctx">The tenant context.</param>
    /// <returns>The tenant's trust ID.</returns>
    public static TrustId GetRequiredTrustId(this ITenantContext ctx)
        => ctx.TrustId
           ?? throw new InvalidOperationException("No tenant_id claim present on the current principal.");
}
