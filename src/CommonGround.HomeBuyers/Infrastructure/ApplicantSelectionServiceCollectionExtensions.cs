using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.HomeBuyers.Infrastructure;

/// <summary>DI registration for the ApplicantSelection bounded context.</summary>
public static class ApplicantSelectionServiceCollectionExtensions
{
    /// <summary>Adds ApplicantSelection services to the service collection.</summary>
    public static IServiceCollection AddApplicantSelection(this IServiceCollection services)
    {
        return services;
    }
}