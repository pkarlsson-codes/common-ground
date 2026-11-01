using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.SharedKernel.Extensions;

/// <summary>
/// Read-only aggregate loading for handlers that decide with an aggregate they don't write to.
/// </summary>
public static class QuerySessionExtensions
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
}
