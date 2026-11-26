using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace CommonGround.API.IntegrationTests;

/// <summary>
/// The durable host with demo mode on, in its own database. Sample trusts need the durable host: their seed
/// runs wait for events to reach every context. A second durable host on the shared database would run a
/// second async daemon, so this one gets its own container.
/// </summary>
public class DemoApiFactory : DurableApiFactory
{
    /// <inheritdoc />
    /// <remarks>Leaves daemon polling to demo mode, so the sample trust test measures what a demo server does.</remarks>
    protected override void ConfigureWolverine(IServiceCollection services) =>
        services.AddWolverineExtension<SoloDurabilityExtension>();

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Demo:Enabled"] = "true",
        }));
    }
}

/// <summary>Defines the xUnit collection that shares a single <see cref="DemoApiFactory"/> instance.</summary>
[CollectionDefinition(Name)]
public class DemoIntegrationCollection : ICollectionFixture<DemoApiFactory>
{
    public const string Name = "demo-integration";
}
