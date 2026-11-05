using CommonGround.PropertyRegistry.Infrastructure.Seeding;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Runs the development seeders after the application has started, once the event subscription
/// and Wolverine are running.
/// </summary>
internal static class DevelopmentSeeder
{
    /// <summary>
    /// Registers the development seeders. They run in the order registered here, and each starts
    /// once propagation from the ones before it has gone idle:
    /// <list type="number">
    /// <item>Trusts, which everything else belongs to.</item>
    /// <item>Land parcels.</item>
    /// <item>Homes, on the land parcels.</item>
    /// <item>Development projects, on the land parcels and their homes.</item>
    /// <item>Users, assigned to their trust.</item>
    /// <item>Households and their applications.</item>
    /// <item>Ground lease templates.</item>
    /// <item>Home selections, choosing households for homes, which starts their purchases.</item>
    /// <item>Purchase workflow progress, on the purchases those selections started; closing one creates its ground lease.</item>
    /// <item>Ground leases: activity on the leases closed purchases created, and leases created directly.</item>
    /// <item>Stewardship cases: inspections, occupancy verifications and lease violations on the cases those leases opened.</item>
    /// </list>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDevelopmentSeeders(this IServiceCollection services)
    {
        services.AddSingleton<IPropagationProbe, MartenWolverinePropagationProbe>();
        services.AddSingleton<PropagationWaiter>(sp => new PropagationWaiter(sp.GetRequiredService<IPropagationProbe>()));

        services.AddTransient<IDevelopmentSeeder, TrustsSeeder>();
        services.AddTransient<IDevelopmentSeeder, LandParcelsSeeder>();
        services.AddTransient<IDevelopmentSeeder, HomesSeeder>();
        services.AddTransient<IDevelopmentSeeder, DevelopmentProjectsSeeder>();
        services.AddTransient<IDevelopmentSeeder, UsersSeeder>();
        services.AddTransient<IDevelopmentSeeder, HouseholdsSeeder>();
        services.AddTransient<IDevelopmentSeeder, GroundLeaseTemplatesSeeder>();
        services.AddTransient<IDevelopmentSeeder, HomeSelectionsSeeder>();
        services.AddTransient<IDevelopmentSeeder, PurchaseWorkflowsSeeder>();
        services.AddTransient<IDevelopmentSeeder, GroundLeasesSeeder>();
        services.AddTransient<IDevelopmentSeeder, StewardshipCasesSeeder>();
        return services;
    }

    /// <summary>Runs the registered <see cref="IDevelopmentSeeder"/>s in development.</summary>
    /// <param name="app">The web application instance.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when all development seeders have finished.</returns>
    public static async Task RunAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("CommonGround.DevelopmentSeeder");
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        var propagation = scope.ServiceProvider.GetRequiredService<PropagationWaiter>();

        foreach (var seeder in scope.ServiceProvider.GetServices<IDevelopmentSeeder>())
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Running development seeder {SeederType}", seeder.GetType().Name);
            }

            await seeder.SeedAsync(store, cancellationToken).ConfigureAwait(false);

            // The next seeder may build on this one's propagated records, or set the clock back.
            await propagation.WaitForIdleAsync(cancellationToken).ConfigureAwait(false);
        }

        logger.LogInformation("Completed development seeders.");
    }
}
