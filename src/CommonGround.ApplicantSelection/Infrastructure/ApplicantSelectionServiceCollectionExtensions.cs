using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.ApplicantSelection.Infrastructure;

/// <summary>DI registration for the ApplicantSelection bounded context.</summary>
public static class ApplicantSelectionServiceCollectionExtensions
{
    /// <summary>Adds ApplicantSelection services to the service collection.</summary>
    public static IServiceCollection AddApplicantSelection(this IServiceCollection services)
    {
        return services;
    }
}