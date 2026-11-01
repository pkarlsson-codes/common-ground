using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.SharedKernel.Extensions;

/// <summary>
/// Extension methods for <see cref="IDocumentSession"/> that reduce boilerplate when persisting aggregates.
/// </summary>
public static class DocumentSessionExtensions
{
    /// <summary>
    /// Loads an aggregate by replaying its event stream. Throws if not found.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the aggregate to load.</typeparam>
    /// <param name="session">The document session to use for loading the stream.</param>
    /// <param name="id">The aggregate's identity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reconstituted aggregate.</returns>
    /// <exception cref="AggregateNotFoundException">Thrown when no stream exists for the given id.</exception>
    public static Task<TAggregate> LoadAggregateAsync<TAggregate>(
        this IQuerySession session,
        IEntityId id,
        CancellationToken cancellationToken = default)
        where TAggregate : class
    {
        ArgumentNullException.ThrowIfNull(id);
        return session.LoadAggregateAsync<TAggregate>(id.Value, cancellationToken);
    }

    /// <summary>
    /// Loads an aggregate, read-only, by replaying the stream with the given key. Throws if not found.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the aggregate to load.</typeparam>
    /// <param name="session">The session to load the stream with.</param>
    /// <param name="streamKey">The aggregate's stream key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reconstituted aggregate.</returns>
    /// <exception cref="AggregateNotFoundException">Thrown when no stream exists for the given key.</exception>
    public static async Task<TAggregate> LoadAggregateAsync<TAggregate>(
        this IQuerySession session,
        string streamKey,
        CancellationToken cancellationToken = default)
        where TAggregate : class
    {
        ArgumentNullException.ThrowIfNull(session);

        var aggregate = await session.Events
            .AggregateStreamAsync<TAggregate>(streamKey, token: cancellationToken)
            .ConfigureAwait(false);
        return aggregate ?? throw new AggregateNotFoundException(typeof(TAggregate), streamKey);
    }

    /// <summary>
    /// Validates invariants, starts a new event stream for <typeparamref name="TAggregate"/>, and marks events as committed.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the aggregate being created.</typeparam>
    /// <typeparam name="TEntityId">The type of the aggregate's unique identifier.</typeparam>
    /// <param name="session">The document session to use for starting the stream.</param>
    /// <param name="aggregate">The aggregate to create and persist.</param>
    public static void CreateAggregate<TAggregate, TEntityId>(
        this IDocumentSession session,
        TAggregate aggregate)
        where TAggregate : AggregateRoot<TEntityId>
        where TEntityId : struct, IEntityId<TEntityId>
    {
        aggregate.ValidateInvariants();
        session.Events.StartStream<TAggregate>(aggregate.Id.Value, [.. aggregate.UncommittedEvents]);
        aggregate.UncommittedEvents.MarkAsCommitted();
    }

    /// <summary>
    /// Validates invariants, appends uncommitted events to an existing stream, and marks events as committed.
    /// </summary>
    /// <typeparam name="TIdentity">The type of the aggregate's unique identifier.</typeparam>
    /// <param name="session">The document session to use for appending events.</param>
    /// <param name="aggregate">The aggregate whose events should be appended.</param>
    public static void UpdateAggregate<TIdentity>(
        this IDocumentSession session,
        AggregateRoot<TIdentity> aggregate)
        where TIdentity : struct, IEntityId<TIdentity>
    {
        aggregate.ValidateInvariants();
        session.Events.Append(aggregate.Id.Value, [.. aggregate.UncommittedEvents]);
        aggregate.UncommittedEvents.MarkAsCommitted();
    }
}
