using Microsoft.Extensions.Options;

namespace CommonGround.API.Demo;

/// <summary>
/// In demo mode, deletes sandboxes past their delete date once an hour, starting when the API has started.
/// </summary>
/// <remarks>
/// A timer, not a Wolverine scheduled message: a sample trust's seed run waits for Wolverine's inbox to be empty,
/// and a durable message stays in the inbox while its handler runs.
/// </remarks>
/// <param name="scopes">Creates a scope per pass.</param>
/// <param name="lifetime">Tells when the API has started.</param>
/// <param name="demo">The demo options; the worker does nothing when demo mode is off.</param>
/// <param name="logger">The logger.</param>
public sealed class DemoSandboxCleanup(
    IServiceScopeFactory scopes,
    IHostApplicationLifetime lifetime,
    IOptions<DemoOptions> demo,
    ILogger<DemoSandboxCleanup> logger) : BackgroundService
{
    /// <summary>How often the worker looks for sandboxes to delete.</summary>
    public static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!demo.Value.Enabled)
        {
            return;
        }

        // Deleting takes members out through Wolverine, which starts with the host.
        var started = new TaskCompletionSource();
        using (lifetime.ApplicationStarted.Register(() => started.TrySetResult()))
        using (stoppingToken.Register(() => started.TrySetCanceled(stoppingToken)))
        {
            await started.Task.ConfigureAwait(false);
        }

        using var timer = new PeriodicTimer(Interval);
        try
        {
            do
            {
                // An exception that leaves ExecuteAsync stops the whole API.
                try
                {
                    using var scope = scopes.CreateScope();
                    await scope.ServiceProvider.GetRequiredService<DemoSandboxDeletion>().DeleteDueAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Could not look for demo sandboxes to delete; trying again in {Interval}", Interval);
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            // Stopping.
        }
    }
}
