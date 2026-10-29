using CommonGround.SharedKernel.Domain.Services;
using CommonGround.TrustManagement.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.TrustManagement.Infrastructure;

/// <summary>DI registration for the TrustManagement bounded context.</summary>
public static class TrustManagementServiceCollectionExtensions
{
    /// <summary>Adds TrustManagement services to the service collection.</summary>
    public static IServiceCollection AddTrustManagement(this IServiceCollection services)
    {
        services.AddSingleton<ITrustClock, TrustClock>();
        return services;
    }
}
