using CommonGround.SharedKernel.Domain.Errors;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.SharedKernel.Domain;

/// <summary>
/// An IANA time zone, e.g. "America/Denver". Turns instants into the calendar dates people in
/// that zone see, so "today" and dates derived from timestamps don't follow the server's UTC clock.
/// </summary>
public sealed class IanaTimeZone : ValueObject
{
    private IanaTimeZone(string value, TimeZoneInfo info)
    {
        Value = value;
        Info = info;
    }

    /// <summary>Gets the IANA time zone id.</summary>
    public string Value { get; }

    /// <summary>Gets the resolved system time zone.</summary>
    public TimeZoneInfo Info { get; }

    /// <summary>Creates and validates a new <see cref="IanaTimeZone"/>.</summary>
    /// <param name="value">An IANA time zone id, e.g. "America/Denver".</param>
    /// <returns>The time zone.</returns>
    /// <exception cref="BusinessRuleViolationException">The value is empty or not a known IANA time zone id.</exception>
    public static IanaTimeZone Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw BusinessRuleViolation.TimeZone.Required();
        }

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(value, out var info) || !info.HasIanaId)
        {
            throw BusinessRuleViolation.TimeZone.Unknown(value);
        }

        return new IanaTimeZone(value, info);
    }

    /// <summary>Returns the calendar date in this time zone at the given instant.</summary>
    /// <param name="instant">The instant to convert.</param>
    /// <returns>The local calendar date.</returns>
    public DateOnly ToDate(DateTimeOffset instant)
        => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, Info).DateTime);

    /// <summary>Returns the instant at which this time zone's clocks show the given date and time.</summary>
    /// <param name="date">The local calendar date.</param>
    /// <param name="time">The local time of day.</param>
    /// <returns>The instant, in UTC.</returns>
    public DateTimeOffset ToInstant(DateOnly date, TimeOnly time)
    {
        var local = date.ToDateTime(time, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, Info.GetUtcOffset(local)).ToUniversalTime();
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
