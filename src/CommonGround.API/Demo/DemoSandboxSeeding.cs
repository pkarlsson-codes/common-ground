using System.Diagnostics;
using System.Threading.Channels;
using CommonGround.API.Infrastructure.Seeding;
using CommonGround.Identity.Application.Commands;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using Marten;
using Microsoft.Extensions.Options;
using Wolverine;

namespace CommonGround.API.Demo;

/// <summary>The sample trusts waiting to be filled, by trust ID.</summary>
public sealed class DemoSandboxBacklog
{
    private readonly Channel<string> _trustIds = Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleReader = true });

    /// <summary>Queues a sample trust to be filled.</summary>
    /// <param name="trustId">The sandbox's trust ID.</param>
    public void Enqueue(string trustId) => _trustIds.Writer.TryWrite(trustId);

    /// <summary>Reads queued trust IDs until <paramref name="cancellationToken"/> is cancelled.</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The trust IDs in the order they were queued.</returns>
    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken cancellationToken) => _trustIds.Reader.ReadAllAsync(cancellationToken);
}

/// <summary>
/// Fills queued sample trusts one at a time: runs the seeders for the sandbox's seed run, makes the owner a
/// member, and marks the sandbox <see cref="DemoSandboxStatus.Ready"/>, or <see cref="DemoSandboxStatus.Failed"/>
/// if the run throws. After a restart it picks up every sandbox still <see cref="DemoSandboxStatus.Seeding"/>;
/// the seeders skip what the run already seeded.
/// </summary>
/// <remarks>
/// Not a Wolverine message: <see cref="PropagationWaiter"/> waits for Wolverine's inbox to be empty, and a
/// durable message stays in the inbox while its handler runs.
/// </remarks>
/// <param name="queue">The queue of sample trusts.</param>
/// <param name="scopes">Creates a scope per sandbox.</param>
/// <param name="runner">Runs the seeders.</param>
/// <param name="lifetime">Tells when the API has started.</param>
/// <param name="demo">The demo options; the worker does nothing when demo mode is off.</param>
/// <param name="logger">The logger.</param>
public sealed class DemoSandboxSeeding(
    DemoSandboxBacklog queue,
    IServiceScopeFactory scopes,
    SeedRunner runner,
    IHostApplicationLifetime lifetime,
    IOptions<DemoOptions> demo,
    ILogger<DemoSandboxSeeding> logger) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!demo.Value.Enabled)
        {
            return;
        }

        // The seeders wait for the async daemon and Wolverine, which start with the host.
        var started = new TaskCompletionSource();
        using (lifetime.ApplicationStarted.Register(() => started.TrySetResult()))
        using (stoppingToken.Register(() => started.TrySetCanceled(stoppingToken)))
        {
            await started.Task.ConfigureAwait(false);
        }

        // An exception that leaves ExecuteAsync stops the whole API, so each step logs its own failure.
        try
        {
            await ResumeUnfinishedAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Could not load demo sandboxes left seeding by the last run of the API; they stay seeding until the next start.");
        }

        await foreach (var trustId in queue.ReadAllAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                await SeedAsync(trustId, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Demo sandbox {TrustId}: could not record the outcome of its seed run; it stays seeding until the next start.", trustId);
            }
        }
    }

    private async Task ResumeUnfinishedAsync(CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var session = scope.ServiceProvider.GetRequiredService<IDocumentStore>().QuerySession();
        await using var sessionDisposal = session.ConfigureAwait(false);
        var unfinished = await session.Query<DemoSandbox>()
            .Where(s => s.Status == DemoSandboxStatus.Seeding)
            .OrderBy(s => s.CreatedAt)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        foreach (var trustId in unfinished)
        {
            queue.Enqueue(trustId);
        }
    }

    private async Task SeedAsync(string trustId, CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        var session = store.LightweightSession();
        await using var sessionDisposal = session.ConfigureAwait(false);

        var sandbox = await session.LoadAsync<DemoSandbox>(trustId, cancellationToken).ConfigureAwait(false);
        if (sandbox is not { Status: DemoSandboxStatus.Seeding, RunKey: not null })
        {
            return;
        }

        var elapsed = Stopwatch.StartNew();
        try
        {
            var run = new SeedRun(sandbox.RunKey) { Trust = new SeedRunTrust(TrustId.Create(trustId), SampleTrust.Name) };
            await runner.RunAsync(run, cancellationToken).ConfigureAwait(false);

            var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
            await bus.InvokeAsync(new AssignUserToTrustIfNone(UserId.Create(sandbox.OwnerUserId), TrustId.Create(trustId)), cancellationToken).ConfigureAwait(false);

            sandbox.Status = DemoSandboxStatus.Ready;
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Demo sandbox {TrustId} ready after {ElapsedSeconds:0.0} s (run {RunKey})", trustId, elapsed.Elapsed.TotalSeconds, sandbox.RunKey);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            sandbox.Status = DemoSandboxStatus.Failed;
            logger.LogError(ex, "Demo sandbox {TrustId} failed after {ElapsedSeconds:0.0} s (run {RunKey})", trustId, elapsed.Elapsed.TotalSeconds, sandbox.RunKey);
        }

        session.Store(sandbox);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
