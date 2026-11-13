using CommonGround.PropertyRegistry.Infrastructure.Seeding;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>Registers the development seeders and the <see cref="SeedRunner"/> that runs them.</summary>
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
    /// <item>Foreclosure prevention cases on those leases, and the interventions recorded on them.</item>
    /// <item>Resales: homeowners' notices of intent to sell on those leases, and their withdrawals.</item>
    /// <item>Lease fee payments on every lease, with a few leases left behind.</item>
    /// </list>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDevelopmentSeeders(this IServiceCollection services)
    {
        services.AddSingleton<IPropagationProbe, MartenWolverinePropagationProbe>();
        services.AddSingleton<PropagationWaiter>(sp => new PropagationWaiter(sp.GetRequiredService<IPropagationProbe>()));
        services.AddSingleton<SeedRunner>();

        services.AddTransient<IDevelopmentSeeder, TrustsSeeder>();
        services.AddTransient<IDevelopmentSeeder, LandParcelsSeeder>();
        services.AddTransient<IDevelopmentSeeder, HomesSeeder>();
        services.AddTransient<IDevelopmentSeeder, DevelopmentProjectsSeeder>();
        services.AddTransient<IDevelopmentSeeder, UsersSeeder>();
        services.AddTransient<IDevelopmentSeeder, HouseholdsSeeder>();
        services.AddTransient<IDevelopmentSeeder, IncomeLimitsSeeder>();
        services.AddTransient<IDevelopmentSeeder, GroundLeaseTemplatesSeeder>();
        services.AddTransient<IDevelopmentSeeder, HomeSelectionsSeeder>();
        services.AddTransient<IDevelopmentSeeder, PurchaseWorkflowsSeeder>();
        services.AddTransient<IDevelopmentSeeder, GroundLeasesSeeder>();
        services.AddTransient<IDevelopmentSeeder, StewardshipCasesSeeder>();
        services.AddTransient<IDevelopmentSeeder, ForeclosurePreventionCasesSeeder>();
        services.AddTransient<IDevelopmentSeeder, ResalesSeeder>();
        services.AddTransient<IDevelopmentSeeder, LeaseFeesSeeder>();
        return services;
    }
}
