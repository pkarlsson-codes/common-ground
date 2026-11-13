using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Runs the registered <see cref="IDevelopmentSeeder"/>s in order for one <see cref="SeedRun"/>. Needs the
/// event subscription and Wolverine running, so it runs in the started API or in <see cref="SeedCommand"/>.
/// </summary>
/// <remarks>
/// Seeders set their clock with <see cref="SeedClock"/>, which only moves time for their own call chain,
/// so a run can go on while the API serves requests and while another run seeds another trust.
/// </remarks>
/// <param name="scopes">Creates the scope each run resolves its seeders in.</param>
/// <param name="logger">The logger.</param>
public sealed class SeedRunner(IServiceScopeFactory scopes, ILogger<SeedRunner> logger)
{
    /// <summary>Runs every seeder for <paramref name="run"/>, skipping what that run already seeded.</summary>
    /// <param name="run">The seed run.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The IDs of the trusts the run seeded, by trust seed key.</returns>
    public async Task<IReadOnlyDictionary<string, string>> RunAsync(SeedRun run, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(run);

        using var scope = scopes.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        var propagation = scope.ServiceProvider.GetRequiredService<PropagationWaiter>();

        foreach (var seeder in scope.ServiceProvider.GetServices<IDevelopmentSeeder>())
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Seed run {RunKey}: running {SeederType}", run.Key, seeder.GetType().Name);
            }

            await seeder.SeedAsync(run, store, cancellationToken).ConfigureAwait(false);

            // The next seeder may build on this one's propagated records. The waiter waits until nothing at
            // all is left to propagate, so concurrent runs and requests make the wait longer.
            await propagation.WaitForIdleAsync(cancellationToken).ConfigureAwait(false);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Seed run {RunKey}: completed", run.Key);
        }

        var session = store.QuerySession();
        await using var sessionDisposal = session.ConfigureAwait(false);
        var trusts = await session.Query<TrustSeedLookup>()
            .Where(t => t.RunKey == run.Key)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return trusts.ToDictionary(t => t.SeedKey, t => t.TrustId, StringComparer.Ordinal);
    }
}
