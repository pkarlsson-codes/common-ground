using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;
using CommonGround.Stewardship.Domain.StewardshipCases;
using CommonGround.Stewardship.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.Stewardship.Infrastructure;

/// <summary>Extension methods for registering Stewardship bounded context services.</summary>
public static class StewardshipServiceCollectionExtensions
{
    /// <summary>Registers Stewardship domain repositories with the DI container.</summary>
    public static IServiceCollection AddStewardship(this IServiceCollection services)
    {
        services.AddScoped<IStewardshipCaseRepository, StewardshipCaseRepository>();
        services.AddScoped<IForeclosurePreventionCaseRepository, ForeclosurePreventionCaseRepository>();
        return services;
    }
}
