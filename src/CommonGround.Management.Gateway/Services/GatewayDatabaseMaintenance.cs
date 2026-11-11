using CommonGround.Management.Gateway.Configuration;
using Microsoft.Extensions.Options;

namespace CommonGround.Management.Gateway.Services;

/// <summary>
/// Creates the gateway's tables before the server takes requests, then deletes expired
/// sessions every <see cref="TokenStoreOptions.CleanupIntervalMinutes"/>.
/// </summary>
public sealed partial class GatewayDatabaseMaintenance(
    GatewayDatabase database,
    PostgresTokenStore tokenStore,
    IOptions<TokenStoreOptions> options,
    TimeProvider timeProvider,
    ILogger<GatewayDatabaseMaintenance> logger) : IHostedService, IDisposable
{
    private readonly CancellationTokenSource _stopping = new();
    private Task? _cleanup;

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
        _cleanup = CleanUpAsync(_stopping.Token);
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _stopping.CancelAsync().ConfigureAwait(false);
        if (_cleanup is not null)
        {
            await _cleanup.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public void Dispose() => _stopping.Dispose();

    [LoggerMessage(EventId = 20, Level = LogLevel.Debug, Message = "Deleted {Count} expired sessions")]
    private static partial void LogExpiredSessionsDeleted(ILogger logger, int count);

    [LoggerMessage(EventId = 21, Level = LogLevel.Warning, Message = "Deleting expired sessions failed")]
    private static partial void LogCleanupFailed(ILogger logger, Exception exception);

    private async Task CleanUpAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(options.Value.CleanupIntervalMinutes), timeProvider);
        try
        {
            do
            {
                try
                {
                    var deleted = await tokenStore.DeleteExpiredAsync(stoppingToken).ConfigureAwait(false);
                    LogExpiredSessionsDeleted(logger, deleted);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    LogCleanupFailed(logger, ex);
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
