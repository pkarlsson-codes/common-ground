using CommonGround.SharedKernel.Infrastructure.Services;

namespace CommonGround.SharedKernel.Domain.Services;

/// <summary>
/// The app's clock, read by domain events (<c>OccurredAt</c>) and <see cref="ITrustClock"/>
/// without dependency injection.
/// </summary>
/// <remarks>
/// A clock set with <see cref="Use"/> applies to the current call chain only (an <see cref="AsyncLocal{T}"/>),
/// so seeding a trust or handling an event at its own time doesn't move the clock for other requests.
/// Without one, <see cref="Current"/> is the global clock, which <see cref="SetCurrent"/> changes for the whole process.
/// </remarks>
public static class TimeProviderContext
{
    private static readonly AsyncLocal<ITimeProvider?> Scoped = new();
    private static ITimeProvider _global = new SystemTimeProvider();

    /// <summary>Gets the clock for the current call chain: the scoped one if set, otherwise the global one.</summary>
    public static ITimeProvider Current => Scoped.Value ?? _global;

    /// <summary>Gets whether the current call chain has its own clock, set with <see cref="Use"/>.</summary>
    public static bool IsScoped => Scoped.Value is not null;

    /// <summary>
    /// Sets the clock for the current call chain and the work it starts, until the returned scope is disposed.
    /// </summary>
    /// <param name="timeProvider">The clock to use.</param>
    /// <returns>A scope that restores the previous scoped clock when disposed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeProvider"/> is null.</exception>
    public static IDisposable Use(ITimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        var previous = Scoped.Value;
        Scoped.Value = timeProvider;
        return new Scope(previous);
    }

    /// <summary>
    /// Sets the global clock for the whole process. For tests that move time across a whole test host;
    /// app code uses <see cref="Use"/>.
    /// </summary>
    /// <param name="timeProvider">The time provider to set as the global clock.</param>
    /// <exception cref="ArgumentNullException">Thrown when timeProvider is null.</exception>
    public static void SetCurrent(ITimeProvider timeProvider)
    {
        _global = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>Resets the global clock to the system clock.</summary>
    public static void Reset()
    {
        _global = new SystemTimeProvider();
    }

    private sealed class Scope(ITimeProvider? previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Scoped.Value = previous;
        }
    }
}
