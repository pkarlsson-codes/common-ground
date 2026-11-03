using System.Diagnostics;

namespace CommonGround.SharedKernel.Infrastructure.Seeding;

/// <summary>
/// Lets a seeder wait for the system's own event flow (subscriptions and handlers) to finish
/// before it builds on what propagation created or moves the seed clock on.
/// </summary>
/// <remarks>
/// Timeouts use real elapsed time, never <c>TimeProviderContext</c>, which a seeder may have set back.
/// </remarks>
public sealed class PropagationWaiter
{
    // Two idle checks in a row: a handler can commit new events between one check's look at the
    // event store and its look at the inbox.
    private const int IdleChecksRequired = 2;

    private readonly IPropagationProbe _probe;
    private readonly TimeSpan _pollInterval;
    private readonly TimeSpan _timeout;

    /// <summary>Initializes a new instance of the <see cref="PropagationWaiter"/> class.</summary>
    /// <param name="probe">Checks whether propagation is still pending.</param>
    public PropagationWaiter(IPropagationProbe probe)
        : this(probe, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(30))
    {
    }

    /// <summary>Initializes a new instance of the <see cref="PropagationWaiter"/> class.</summary>
    /// <param name="probe">Checks whether propagation is still pending.</param>
    /// <param name="pollInterval">How long to wait between checks.</param>
    /// <param name="timeout">How long to wait before giving up.</param>
    public PropagationWaiter(IPropagationProbe probe, TimeSpan pollInterval, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(probe);
        _probe = probe;
        _pollInterval = pollInterval;
        _timeout = timeout;
    }

    /// <summary>Waits until no events or messages are left to propagate.</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when propagation is idle.</returns>
    /// <exception cref="TimeoutException">Propagation was still pending when the timeout passed.</exception>
    public async Task WaitForIdleAsync(CancellationToken cancellationToken = default)
    {
        var elapsed = Stopwatch.StartNew();
        var idleChecks = 0;
        while (true)
        {
            var pending = await _probe.FindPendingAsync(cancellationToken).ConfigureAwait(false);
            idleChecks = pending is null ? idleChecks + 1 : 0;
            if (idleChecks == IdleChecksRequired)
            {
                return;
            }

            if (elapsed.Elapsed >= _timeout)
            {
                throw new TimeoutException($"Propagation was still pending after {_timeout.TotalSeconds:0.#}s: {pending ?? "settling"}.");
            }

            await Task.Delay(_pollInterval, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Waits until <paramref name="find"/> returns a record, as a check that propagation created it.</summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <param name="description">What is being waited for, e.g. "the purchase workflow for application X".</param>
    /// <param name="find">Looks the record up, returning null while it doesn't exist.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The record.</returns>
    /// <exception cref="TimeoutException">The record didn't appear before the timeout passed.</exception>
    public async Task<T> WaitForAsync<T>(string description, Func<CancellationToken, Task<T?>> find, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(find);

        var elapsed = Stopwatch.StartNew();
        while (true)
        {
            if (await find(cancellationToken).ConfigureAwait(false) is { } found)
            {
                return found;
            }

            if (elapsed.Elapsed >= _timeout)
            {
                throw new TimeoutException($"Timed out after {_timeout.TotalSeconds:0.#}s waiting for {description}.");
            }

            await Task.Delay(_pollInterval, cancellationToken).ConfigureAwait(false);
        }
    }
}
