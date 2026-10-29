namespace CommonGround.SharedKernel.Domain.Services;

/// <summary>
/// Calendar dates as seen in a trust's time zone. Use it for "today" and for dates derived from
/// timestamps; deriving them from UTC puts evening events on the next day.
/// </summary>
public interface ITrustClock
{
    /// <summary>Gets the time zone the trust operates in.</summary>
    /// <param name="trustId">The trust (tenant) identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The trust's time zone.</returns>
    Task<IanaTimeZone> GetTimeZoneAsync(string trustId, CancellationToken cancellationToken = default);

    /// <summary>Gets today's date in the trust's time zone.</summary>
    /// <param name="trustId">The trust (tenant) identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The trust's current calendar date.</returns>
    async Task<DateOnly> TodayAsync(string trustId, CancellationToken cancellationToken = default)
        => await ToTrustDateAsync(trustId, TimeProviderContext.Current.UtcNow, cancellationToken).ConfigureAwait(false);

    /// <summary>Gets the calendar date in the trust's time zone at the given instant.</summary>
    /// <param name="trustId">The trust (tenant) identifier.</param>
    /// <param name="instant">The instant to convert.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The trust's calendar date at that instant.</returns>
    async Task<DateOnly> ToTrustDateAsync(string trustId, DateTimeOffset instant, CancellationToken cancellationToken = default)
        => (await GetTimeZoneAsync(trustId, cancellationToken).ConfigureAwait(false)).ToDate(instant);
}
