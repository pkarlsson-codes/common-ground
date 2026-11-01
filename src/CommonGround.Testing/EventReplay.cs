using System.Collections.Concurrent;
using System.Reflection;
using JasperFx.Events;
using JasperFx.Events.Aggregation;

namespace CommonGround.Testing;

/// <summary>
/// Builds aggregate state for unit tests by replaying events through the same source-generated evolver
/// Marten uses to load the aggregate. A test that replays an event whose <c>Apply</c> is missing, or not
/// public, fails the way loading the aggregate would.
/// </summary>
public static class EventReplay
{
    private static readonly ConcurrentDictionary<Type, Func<object?, object, object?>> Evolvers = new();

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
        where TAggregate : class
    {
        ArgumentNullException.ThrowIfNull(events);

        var evolve = Evolvers.GetOrAdd(typeof(TAggregate), BuildEvolver);
        object? current = aggregate;
        foreach (var data in events)
        {
            ArgumentNullException.ThrowIfNull(data);
            current = evolve(current, Activator.CreateInstance(typeof(Event<>).MakeGenericType(data.GetType()), data)!);
        }

        return (TAggregate?)current;
    }

    private static Func<object?, object, object?> BuildEvolver(Type aggregateType)
    {
        var registration = aggregateType.Assembly.GetCustomAttributesData()
            .Where(a => a.AttributeType == typeof(GeneratedEvolverAttribute))
            .Select(a => a.ConstructorArguments.Select(arg => (Type)arg.Value!).ToArray())
            .SingleOrDefault(types => types[0] == aggregateType)
            ?? throw new InvalidOperationException(
                $"Marten's source generator emitted no evolver for {aggregateType.Name}. Check that its Create and Apply methods are public.");

        var evolverType = registration[^1];
        var evolverInterface = evolverType.GetInterfaces()
            .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGeneratedSyncEvolver<,>));
        var idType = evolverInterface.GetGenericArguments()[1];
        var evolver = Activator.CreateInstance(evolverType, nonPublic: true)!;
        var evolve = evolverInterface.GetMethod(nameof(IGeneratedSyncEvolver<object, object>.Evolve))!;
        var id = idType.IsValueType ? Activator.CreateInstance(idType) : null;

        return (snapshot, @event) =>
        {
            try
            {
                return evolve.Invoke(evolver, [snapshot, id, @event]);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        };
    }
}
