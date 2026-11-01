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
    /// <summary>
    /// Asks the stream's aggregate for a decision and appends what it decides: one event, several, or
    /// nothing (<see langword="null"/> or empty).
    /// </summary>
    /// <typeparam name="TAggregate">The aggregate type.</typeparam>
    /// <param name="stream">The stream, fetched for writing.</param>
    /// <param name="decide">The decision to take on the aggregate.</param>
    /// <exception cref="AggregateNotFoundException">The stream doesn't exist.</exception>
    public static void Decide<TAggregate>(this IEventStream<TAggregate> stream, Func<TAggregate, object?> decide)
        where TAggregate : class
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(decide);

        var aggregate = stream.Aggregate
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
}
