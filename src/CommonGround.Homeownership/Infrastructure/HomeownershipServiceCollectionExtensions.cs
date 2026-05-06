using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.Homeownership.Infrastructure;

/// <summary>DI registration for the Homeownership bounded context.</summary>
public static class HomeownershipServiceCollectionExtensions
{
    /// <summary>Adds Homeownership services to the service collection.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHomeownership(this IServiceCollection services)
    {
        return services;
    }
}
