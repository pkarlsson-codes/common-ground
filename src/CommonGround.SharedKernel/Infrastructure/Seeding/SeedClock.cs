using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.Services;

namespace CommonGround.SharedKernel.Infrastructure.Seeding;

/// <summary>
/// Sets the app's clock back while a seeder raises events, so their <c>OccurredAt</c> (and that of
/// anything propagation creates from them) reads as the seeded moment rather than today.
/// </summary>
/// <remarks>
/// The clock is scoped to the seeder's call chain (<see cref="TimeProviderContext.Use"/>), so requests handled
/// while seeding runs keep the real time. Handlers that propagation runs get the seeded moment from the event
/// they handle (its <c>OccurredAt</c>), not from this clock. Wait for propagation to go idle
/// (<see cref="PropagationWaiter.WaitForIdleAsync"/>) before moving the clock on, so those handlers have run
/// before the seeder builds on their records.
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
    /// Runs <paramref name="seed"/> with its call chain's clock under its control, and restores the
    /// previous clock afterwards, whether or not the seed succeeds.
    /// </summary>
    /// <param name="timeZone">The trust's time zone, which seeded dates are local to.</param>
    /// <param name="seed">The seeding to run; it moves the clock with <see cref="MoveTo(DateOnly)"/>.</param>
    /// <returns>A task that completes when the seed has finished.</returns>
    public static async Task RunAsync(IanaTimeZone timeZone, Func<SeedClock, Task> seed)
    {
        ArgumentNullException.ThrowIfNull(timeZone);
        ArgumentNullException.ThrowIfNull(seed);

        var clock = new ControllableTimeProvider(TimeProviderContext.Current.UtcNow);
        using (TimeProviderContext.Use(clock))
        {
            await seed(new SeedClock(clock, timeZone)).ConfigureAwait(false);
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
