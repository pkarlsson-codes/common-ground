using CommonGround.ApplicantSelection.Application.ReadModels;
using CommonGround.ApplicantSelection.Infrastructure.Persistence.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace CommonGround.ApplicantSelection.Infrastructure.Persistence;

/// <summary>Marten schema and projection configuration for the ApplicantSelection bounded context.</summary>
public static class ApplicantSelectionMartenConfiguration
{
    private const string SchemaName = "applicant_selection";

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
        options.Schema.For<HomeAvailabilityClaimView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<HomeLandParcelView>().DatabaseSchemaName(SchemaName);
    }

    private static void ConfigureProjections(StoreOptions options)
    {
        options.Projections.Add<HomeownerApplicationViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HomeownerApplicationListItemViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HomeAvailabilityClaimViewProjection>(ProjectionLifecycle.Inline);
    }
}