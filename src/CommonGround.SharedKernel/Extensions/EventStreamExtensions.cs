using System.Runtime.CompilerServices;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using JasperFx.Events;

namespace CommonGround.SharedKernel.Extensions;

/// <summary>
/// Appends an aggregate's decisions to a stream fetched for writing. Handlers use it where Wolverine's
/// <c>[WriteAggregate]</c> can't load the aggregate for them: several streams at once, or an integration
/// event that names the aggregate by a plain string id.
/// </summary>
public static class EventStreamExtensions
{
    // The aggregate each stream's decisions have evolved so far, and how many of its events that covers.
    private static readonly ConditionalWeakTable<object, Decided> Streams = [];

    /// <summary>
    /// Asks the stream's aggregate for a decision and appends what it decides: one event, several, or
    /// nothing (<see langword="null"/> or empty).
    /// </summary>
    /// <remarks>
    /// The stream's <see cref="IEventStream{T}.Aggregate"/> stays as fetched when events are appended, so
    /// each call first applies what has been appended since the last one, so a second decision in the same
    /// handler sees the state the first one produced. Catching up changes a mutable
    /// aggregate in place, which is safe while aggregates are aggregated live rather than kept as
    /// identity-mapped snapshots.
    /// </remarks>
    /// <typeparam name="TAggregate">The aggregate type.</typeparam>
    /// <param name="stream">The stream, fetched for writing.</param>
    /// <param name="decide">The decision to take on the aggregate.</param>
    /// <exception cref="AggregateNotFoundException">The stream doesn't exist, or an earlier decision deleted it.</exception>
    public static void Decide<TAggregate>(this IEventStream<TAggregate> stream, Func<TAggregate, object?> decide)
        where TAggregate : class
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(decide);

        var decided = Streams.GetValue(stream, s => new Decided(((IEventStream<TAggregate>)s).Aggregate));
        var appended = stream.Events;
        if (decided.Applied < appended.Count)
        {
            decided.Aggregate = AggregateEvolver.Evolve((TAggregate?)decided.Aggregate, appended.Skip(decided.Applied));
            decided.Applied = appended.Count;
        }

        var aggregate = (TAggregate?)decided.Aggregate
            ?? throw new AggregateNotFoundException(typeof(TAggregate), stream.Key ?? stream.Id.ToString());

        switch (decide(aggregate))
        {
            case null:
                break;
            case IEnumerable<object> events:
                stream.AppendMany(events);
                break;
            case var @event:
                stream.AppendOne(@event);
                break;
        }
    }

    private sealed class Decided(object? aggregate)
    {
        public object? Aggregate { get; set; } = aggregate;

        public int Applied { get; set; }
    }
}
