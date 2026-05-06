using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.PropertyRegistry.Infrastructure;

/// <summary>DI registration for the PropertyRegistry bounded context.</summary>
public static class PropertyRegistryServiceCollectionExtensions
{
    /// <summary>Adds PropertyRegistry services to the service collection.</summary>
    public static IServiceCollection AddPropertyRegistry(this IServiceCollection services)
    {
        return services;
    }
}
