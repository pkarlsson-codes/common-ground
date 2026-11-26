using CommonGround.Reminders.Application.ReadModels;
using CommonGround.Reminders.Infrastructure.Persistence.Projections;
using CommonGround.Reminders.Infrastructure.Seeding;
using JasperFx.Events.Projections;
using Marten;

namespace CommonGround.Reminders.Infrastructure.Persistence;

/// <summary>Marten schema and projection configuration for the Reminders bounded context.</summary>
public static class RemindersMartenConfiguration
{
    private const string SchemaName = "reminders";
    private const string SeedSchemaName = "reminders_seed";

    /// <summary>Configures Marten for Reminders.</summary>
    /// <param name="options">The store options.</param>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Schema.For<ReminderView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.IsDone)
            .Index(x => x.RemindOn);
        options.Schema.For<ReminderSeedLookup>()
            .DatabaseSchemaName(SeedSchemaName)
            .Index(x => x.SeedKey);

        options.Projections.Add<ReminderViewProjection>(ProjectionLifecycle.Inline);
    }
}
