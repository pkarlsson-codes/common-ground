using CommonGround.ApplicantSelection.Application.ReadModels;
using CommonGround.ApplicantSelection.Infrastructure.Persistence.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace CommonGround.ApplicantSelection.Infrastructure.Persistence;

/// <summary>Marten schema and projection configuration for the ApplicantSelection bounded context.</summary>
public static class ApplicantSelectionMartenConfiguration
{
    private const string SchemaName = "applicant_selection";

    private static ProjectionLifecycle ProjectionLifecycleForEnvironment =>
        string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Testing", StringComparison.OrdinalIgnoreCase)
            ? ProjectionLifecycle.Inline
            : ProjectionLifecycle.Async;

    /// <summary>Configures Marten for ApplicantSelection.</summary>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        ConfigureReadModels(options);
        ConfigureProjections(options);
    }

    private static void ConfigureReadModels(StoreOptions options)
    {
        options.Schema.For<HomeownerApplicationView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<HomeownerApplicationListItemView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.ApplicantFullName)
            .Index(x => x.ApplicantEmail)
            .Index(x => x.Phase);
    }

    private static void ConfigureProjections(StoreOptions options)
    {
        options.Projections.Add<HomeownerApplicationViewProjection>(ProjectionLifecycleForEnvironment);
        options.Projections.Add<HomeownerApplicationListItemViewProjection>(ProjectionLifecycleForEnvironment);
    }
}