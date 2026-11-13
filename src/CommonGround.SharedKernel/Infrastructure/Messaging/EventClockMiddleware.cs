using System.Globalization;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.Services;
using JasperFx.Events;
using Wolverine;

namespace CommonGround.SharedKernel.Infrastructure.Messaging;

/// <summary>
/// Runs a handler with the clock fixed at the time of what caused it: a reaction happens when its cause happened.
/// Events are delivered after they are raised (by the Marten subscription or a durable queue), so a handler's
/// own "now" would be the delivery time. Seeded events, raised under a <see cref="Seeding.SeedClock"/>,
/// get their reactions on the seeded date this way.
/// </summary>
/// <remarks>
/// The handler's time is, in order: the message's own <c>OccurredAt</c> (a domain or integration event);
/// the <see cref="ClockHeader"/> of its envelope, which messages cascaded from a timed handler inherit
/// (<see cref="Register"/>), or the time a scheduled message was due, whichever is later; otherwise the current clock is left alone. A handler run inline
/// (<c>InvokeAsync</c>) by code with a scoped clock already shares that clock, and stamps it on what it cascades.
/// </remarks>
public static class EventClockMiddleware
{
    /// <summary>The envelope header carrying the time a cascaded message's handler runs at.</summary>
    public const string ClockHeader = "cg-clock";

    /// <summary>Adds the middleware to every handler and passes <see cref="ClockHeader"/> on to cascaded messages.</summary>
    /// <param name="options">The Wolverine options.</param>
    public static void Register(WolverineOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Policies.AddMiddleware(typeof(EventClockMiddleware), _ => true);
        options.Policies.PropagateIncomingHeaderToOutgoing(ClockHeader);
    }

    /// <summary>
    /// Sets the handler's clock and stamps it on the envelope, so cascaded messages inherit it. Synchronous on
    /// purpose: an <see cref="AsyncLocal{T}"/> set in a synchronous call stays set in the calling (generated)
    /// handler method.
    /// </summary>
    /// <param name="envelope">The envelope being handled.</param>
    /// <returns>The scope <see cref="Finally"/> ends.</returns>
    public static EventClockScope Before(Envelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var at = OccurredAt(envelope.Message) ?? Later(FromHeader(envelope), envelope.ScheduledTime);
        if (at is { } handledAt)
        {
            envelope.Headers[ClockHeader] = handledAt.ToString("O", CultureInfo.InvariantCulture);
            return new EventClockScope(TimeProviderContext.Use(new ControllableTimeProvider(handledAt)));
        }

        if (TimeProviderContext.IsScoped)
        {
            envelope.Headers[ClockHeader] = TimeProviderContext.Current.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        }

        return new EventClockScope(null);
    }

    /// <summary>Restores the clock after the handler has run, whether or not it succeeded.</summary>
    /// <param name="scope">The scope <see cref="Before"/> started.</param>
    public static void Finally(EventClockScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        scope.End();
    }

    /// <summary>The time an event message occurred, or <see langword="null"/> for any other message.</summary>
    /// <param name="message">The message.</param>
    /// <returns>The event's <c>OccurredAt</c>.</returns>
    public static DateTimeOffset? OccurredAt(object? message) => message switch
    {
        IDomainEvent domainEvent => domainEvent.OccurredAt,
        IntegrationEvent integrationEvent => integrationEvent.OccurredAt,
        IEvent wrapped => OccurredAt(wrapped.Data),
        _ => null,
    };

    // A scheduled message happens when it is due, which is never before its cause.
    private static DateTimeOffset? Later(DateTimeOffset? cause, DateTimeOffset? due) =>
        cause is null || (due is not null && due > cause) ? due : cause;

    private static DateTimeOffset? FromHeader(Envelope envelope) =>
        envelope.TryGetHeader(ClockHeader, out var value)
            && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var at)
            ? at
            : null;
}

/// <summary>The clock scope <see cref="EventClockMiddleware"/> holds while a handler runs.</summary>
/// <param name="scope">The scope to end, or <see langword="null"/> when the clock wasn't changed.</param>
public sealed class EventClockScope(IDisposable? scope)
{
    /// <summary>Restores the clock that was current before the handler.</summary>
    public void End() => scope?.Dispose();
}
