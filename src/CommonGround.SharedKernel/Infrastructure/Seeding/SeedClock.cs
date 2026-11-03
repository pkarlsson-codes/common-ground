using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.Services;

namespace CommonGround.SharedKernel.Infrastructure.Seeding;

/// <summary>
/// Sets the app's clock back while a seeder raises events, so their <c>OccurredAt</c> (and that of
/// anything propagation creates from them) reads as the seeded moment rather than today.
/// </summary>
/// <remarks>
/// The clock is <see cref="TimeProviderContext"/>, shared by the whole app. That is safe only
/// because nothing else is handled while seeding runs: seeding runs before the app takes requests
/// and must never run alongside a live system. Wait for propagation to go idle
/// (<see cref="PropagationWaiter.WaitForIdleAsync"/>) before moving the clock on.
/// </remarks>
public sealed class SeedClock
{
    /// <summary>The local time of day a seeded date is placed at.</summary>
    public static readonly TimeOnly SeededTimeOfDay = new(10, 0);

    private readonly ControllableTimeProvider _clock;
    private readonly IanaTimeZone _timeZone;

    private SeedClock(ControllableTimeProvider clock, IanaTimeZone timeZone)
    {
        _clock = clock;
        _timeZone = timeZone;
    }

    /// <summary>
    /// Runs <paramref name="seed"/> with the app's clock under its control, and restores the
    /// original clock afterwards, whether or not the seed succeeds.
    /// </summary>
    /// <param name="timeZone">The trust's time zone, which seeded dates are local to.</param>
    /// <param name="seed">The seeding to run; it moves the clock with <see cref="MoveTo(DateOnly)"/>.</param>
    /// <returns>A task that completes when the seed has finished.</returns>
    public static async Task RunAsync(IanaTimeZone timeZone, Func<SeedClock, Task> seed)
    {
        ArgumentNullException.ThrowIfNull(timeZone);
        ArgumentNullException.ThrowIfNull(seed);

        var original = TimeProviderContext.Current;
        var clock = new ControllableTimeProvider(original.UtcNow);
        TimeProviderContext.SetCurrent(clock);
        try
        {
            await seed(new SeedClock(clock, timeZone)).ConfigureAwait(false);
        }
        finally
        {
            TimeProviderContext.SetCurrent(original);
        }
    }

    /// <summary>Moves the clock to <see cref="SeededTimeOfDay"/> on the given date in the trust's time zone.</summary>
    /// <param name="date">The date being seeded.</param>
    public void MoveTo(DateOnly date) => MoveTo(date, SeededTimeOfDay);

    /// <summary>Moves the clock to the given date and time in the trust's time zone.</summary>
    /// <param name="date">The date being seeded.</param>
    /// <param name="time">The local time of day.</param>
    public void MoveTo(DateOnly date, TimeOnly time) => _clock.SetTime(_timeZone.ToInstant(date, time));
}
