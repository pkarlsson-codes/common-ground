using Marten;
using Microsoft.Extensions.Options;

namespace CommonGround.API.Demo;

/// <summary>Registers demo mode.</summary>
internal static class DemoServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="DemoOptions"/>, checked at startup, and the <see cref="DemoSandbox"/> document. In demo
    /// mode, the workers that fill sample trusts and delete expired sandboxes run, and the daemon polls faster.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDemo(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DemoOptions>()
            .Bind(configuration.GetSection(DemoOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<Microsoft.Extensions.Options.IValidateOptions<DemoOptions>, DemoOptionsValidator>();

        services.ConfigureMarten(options => options.Schema.For<DemoSandbox>().DatabaseSchemaName("demo"));

        // Registered either way and read DemoOptions when they run: config added by a test host isn't
        // visible while services are registered.
        services.AddSingleton<DemoSandboxBacklog>();
        services.AddHostedService<DemoSandboxSeeding>();
        services.AddScoped<DemoSandboxDeletion>();
        services.AddHostedService<DemoSandboxCleanup>();
        services.AddSingleton<IConfigureMarten, DemoPolling>();

        return services;
    }

    /// <summary>
    /// In demo mode, polls the async daemon every 50 ms. A sample trust's seed run waits for the daemon after
    /// every seed entry: at the default 1 s polling a run takes about 3 minutes, at 100 ms about 21 s, at 50 ms
    /// about 15 s.
    /// </summary>
    private sealed class DemoPolling(IOptions<DemoOptions> demo) : IConfigureMarten
    {
        public void Configure(IServiceProvider services, StoreOptions options)
        {
            if (!demo.Value.Enabled)
            {
                return;
            }

            options.Projections.SlowPollingTime = TimeSpan.FromMilliseconds(50);
            options.Projections.FastPollingTime = TimeSpan.FromMilliseconds(50);
        }
    }
}
