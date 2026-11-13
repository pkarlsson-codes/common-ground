using Marten;

namespace CommonGround.SharedKernel.Infrastructure.Seeding;

/// <summary>
/// Seeds development data. Runs after the app has started, so the event subscription and
/// Wolverine are running and a seeder can wait on propagation.
/// </summary>
/// <remarks>
/// Not Marten's <c>IInitialData</c>: Marten runs those while the host starts, before propagation is
/// running, and in every environment.
/// </remarks>
public interface IDevelopmentSeeder
{
    /// <summary>Seeds this seeder's data, skipping anything already seeded.</summary>
    /// <param name="run">The seed run. Trusts are looked up in this run's namespace.</param>
    /// <param name="store">The document store.</param>
    /// <param name="cancellation">A cancellation token.</param>
    /// <returns>A task that completes when seeding has finished.</returns>
    Task SeedAsync(SeedRun run, IDocumentStore store, CancellationToken cancellation);
}
