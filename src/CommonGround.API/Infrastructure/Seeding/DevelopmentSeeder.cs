using Marten;
using Marten.Schema;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Executes development-only data seeders after the application has started.
/// </summary>
internal static class DevelopmentSeeder
{
    /// <summary>
    /// Runs all registered Marten <see cref="IInitialData"/> seeders in development.
    /// </summary>
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
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DevelopmentSeeder");
        var seeders = scope.ServiceProvider.GetServices<IInitialData>().ToArray();

        if (seeders.Length == 0)
        {
            logger.LogInformation("No development seeders were registered.");
            return;
        }

        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();

        foreach (var seeder in seeders)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Running development seeder {SeederType}", seeder.GetType().Name);
            }

            await seeder.Populate(store, cancellationToken).ConfigureAwait(false);
        }

        logger.LogInformation("Completed development seeders.");
    }
}
