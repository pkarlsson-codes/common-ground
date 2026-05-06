using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.Stewardship.Infrastructure;

/// <summary>Extension methods for registering Stewardship bounded context services.</summary>
public static class StewardshipServiceCollectionExtensions
{
    /// <summary>Registers Stewardship domain services with the DI container.</summary>
    public static IServiceCollection AddStewardship(this IServiceCollection services)
    {
        return services;
    }
}
