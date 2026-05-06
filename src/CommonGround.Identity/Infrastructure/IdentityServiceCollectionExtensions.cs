using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.Identity.Infrastructure;

/// <summary>DI registration for the Identity bounded context.</summary>
public static class IdentityServiceCollectionExtensions
{
    /// <summary>Adds Identity services to the service collection.</summary>
    public static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        return services;
    }
}
