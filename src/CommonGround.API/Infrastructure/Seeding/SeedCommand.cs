using CommonGround.SharedKernel.Infrastructure.Seeding;
using Marten;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// <c>dotnet run --project src/CommonGround.API -- seed</c>: starts the API's services without a web server
/// (Marten, the async daemon and Wolverine run as they do in the API), runs the development seeders and exits.
/// Exits 1 outside Development or if a seeder fails.
/// </summary>
internal static class SeedCommand
{
    /// <summary>Whether the command line asks for the seed command.</summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns><see langword="true"/> if the first argument is <c>seed</c>.</returns>
    public static bool IsRequested(string[] args) =>
        args.Length > 0 && string.Equals(args[0], "seed", StringComparison.OrdinalIgnoreCase);

    /// <summary>Configures the host for seeding: no web server and fast daemon polling.</summary>
    /// <param name="builder">The web application builder, after the API's services are added.</param>
    public static void Configure(WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Replaces Kestrel, so the command doesn't bind the API's port and can run while the API is up.
        builder.Services.AddSingleton<IServer, NoServer>();

        // The seeders wait for the daemon after every seed entry. The defaults (1 s slow, 250 ms fast)
        // make each wait about 1 s; at 50 ms a fresh seed takes about 12 s instead of 160 s.
        builder.Services.ConfigureMarten(opts =>
        {
            opts.Projections.SlowPollingTime = TimeSpan.FromMilliseconds(50);
            opts.Projections.FastPollingTime = TimeSpan.FromMilliseconds(50);
        });
    }

    /// <summary>Starts the host, runs the development seeders and stops the host.</summary>
    /// <param name="app">The web application, built after <see cref="Configure"/>.</param>
    /// <returns>The process exit code: 0 on success, 1 otherwise.</returns>
    public static async Task<int> RunAsync(WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("CommonGround.SeedCommand");
        if (!app.Environment.IsDevelopment())
        {
            logger.LogError("The seed command only runs in Development; the environment is {Environment}.", app.Environment.EnvironmentName);
            return 1;
        }

        await app.StartAsync().ConfigureAwait(false);
        try
        {
            await app.Services.GetRequiredService<SeedRunner>()
                .RunAsync(SeedRun.Development, app.Lifetime.ApplicationStopping)
                .ConfigureAwait(false);
            return 0;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Seeding failed.");
            return 1;
        }
        finally
        {
            await app.StopAsync().ConfigureAwait(false);
        }
    }

    /// <summary>A server that accepts no connections.</summary>
    private sealed class NoServer : IServer
    {
        public IFeatureCollection Features { get; } = new FeatureCollection();

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
            where TContext : notnull => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public void Dispose()
        {
        }
    }
}
