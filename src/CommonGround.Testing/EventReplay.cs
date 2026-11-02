using CommonGround.SharedKernel.Domain;

namespace CommonGround.Testing;

/// <summary>
/// Builds aggregate state for unit tests by replaying events through <see cref="AggregateEvolver"/>, the
/// source-generated evolver Marten uses to load the aggregate. A test that replays an event whose
/// <c>Apply</c> is missing, or not public, fails the way loading the aggregate would.
/// </summary>
public static class EventReplay
{
    /// <summary>Builds an aggregate from the events of its stream, first event first.</summary>
    /// <typeparam name="TAggregate">The aggregate type.</typeparam>
    /// <param name="events">The stream's events.</param>
    /// <returns>The aggregate.</returns>
    public static TAggregate Replay<TAggregate>(params IEnumerable<object> events)
        where TAggregate : class =>
        Evolve<TAggregate>(null, events)
            ?? throw new InvalidOperationException($"Replaying the events left no {typeof(TAggregate).Name}.");

    /// <summary>Applies further events to an aggregate, as a later load of its stream would.</summary>
    /// <typeparam name="TAggregate">The aggregate type.</typeparam>
    /// <param name="aggregate">The aggregate so far, or <see langword="null"/> before the first event.</param>
    /// <param name="events">The events to apply, in order.</param>
    /// <returns>The evolved aggregate, or <see langword="null"/> when an event deletes it.</returns>
    public static TAggregate? Evolve<TAggregate>(this TAggregate? aggregate, params IEnumerable<object> events)
        where TAggregate : class =>
        AggregateEvolver.Evolve(aggregate, events);

    /// <summary>
    /// Takes a decision on an aggregate and evolves it with what was decided: one event, several, or
    /// nothing (<see langword="null"/> or empty).
    /// </summary>
    /// <typeparam name="TAggregate">The aggregate type.</typeparam>
    /// <param name="aggregate">The aggregate before the decision.</param>
    /// <param name="decision">The decision to take.</param>
    /// <returns>The aggregate after the decided events, and the events.</returns>
    public static (TAggregate Aggregate, IReadOnlyList<object> Events) Decide<TAggregate>(
        this TAggregate aggregate,
        Func<TAggregate, object?> decision)
        where TAggregate : class
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        ArgumentNullException.ThrowIfNull(decision);

        IReadOnlyList<object> events = decision(aggregate) switch
        {
            null => [],
            IEnumerable<object> many => [.. many],
            var one => [one],
        };

        return (aggregate.Evolve(events) ?? aggregate, events);
    }
}
