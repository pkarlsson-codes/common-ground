using CommonGround.TrustManagement.Domain.Trusts;
using CommonGround.TrustManagement.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.TrustManagement.Infrastructure;

/// <summary>DI registration for the TrustManagement bounded context.</summary>
public static class TrustManagementServiceCollectionExtensions
{
    /// <summary>Adds TrustManagement services to the service collection.</summary>
    public static IServiceCollection AddTrustManagement(this IServiceCollection services)
    {
        services.AddScoped<ITrustRepository, TrustRepository>();
        return services;
    }
}
