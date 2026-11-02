using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;
using JasperFx.Events;
using JasperFx.Events.Aggregation;

namespace CommonGround.SharedKernel.Domain;

/// <summary>
/// Applies events to an aggregate through the evolver Marten's source generator emits from its public
/// <c>Create</c> and <c>Apply</c> methods, the same code Marten runs when it loads the aggregate.
/// </summary>
public static class AggregateEvolver
{
    private static readonly ConcurrentDictionary<Type, Func<object?, IEvent, object?>> Evolvers = new();

    /// <summary>Applies events to an aggregate, in order, as a later load of its stream would.</summary>
    /// <typeparam name="TAggregate">The aggregate type.</typeparam>
    /// <param name="aggregate">The aggregate so far, or <see langword="null"/> before the first event.</param>
    /// <param name="events">The events to apply: event data, or data already wrapped in an <see cref="IEvent"/>.</param>
    /// <returns>
    /// The evolved aggregate, or <see langword="null"/> when an event deletes it. An aggregate with
    /// <c>void Apply</c> methods is changed in place and returned.
    /// </returns>
    public static TAggregate? Evolve<TAggregate>(TAggregate? aggregate, IEnumerable<object> events)
        where TAggregate : class
    {
        ArgumentNullException.ThrowIfNull(events);

        var evolve = Evolvers.GetOrAdd(typeof(TAggregate), BuildEvolver);
        object? current = aggregate;
        foreach (var data in events)
        {
            ArgumentNullException.ThrowIfNull(data);
            var @event = data as IEvent
                ?? (IEvent)Activator.CreateInstance(typeof(Event<>).MakeGenericType(data.GetType()), data)!;
            current = evolve(current, @event);
        }

        return (TAggregate?)current;
    }

    private static Func<object?, IEvent, object?> BuildEvolver(Type aggregateType)
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
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        };
    }
}
