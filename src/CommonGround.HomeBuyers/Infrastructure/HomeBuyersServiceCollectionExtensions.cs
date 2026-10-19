using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.HomeBuyers.Infrastructure;

/// <summary>DI registration for the HomeBuyers bounded context.</summary>
public static class HomeBuyersServiceCollectionExtensions
{
    /// <summary>Adds HomeBuyers services to the service collection.</summary>
    public static IServiceCollection AddHomeBuyers(this IServiceCollection services)
    {
        return services;
    }
}