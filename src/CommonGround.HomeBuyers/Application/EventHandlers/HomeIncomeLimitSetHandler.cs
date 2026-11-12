using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.PropertyRegistry.Contracts.Events;
using Marten;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>Keeps <see cref="HomeFactsView"/>'s income limit in step with PropertyRegistry's HomeIncomeLimitSet integration event.</summary>
public static class HomeIncomeLimitSetHandler
{
    /// <summary>Loads the home's facts.</summary>
    public static Task<HomeFactsView?> LoadAsync(HomeIncomeLimitSet @event, IQuerySession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);

        return session.LoadAsync<HomeFactsView>(@event.HomeId, cancellationToken);
    }

    /// <summary>
    /// Records the home's new income limit. Throws when HomeBuyers has no facts for the home, so the message
    /// fails and lands in the dead letter queue instead of leaving selection checking the old limit.
    /// </summary>
    public static IMartenOp Handle(HomeIncomeLimitSet @event, HomeFactsView? facts)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (facts is null)
            throw new InvalidOperationException($"HomeBuyers has no facts for home {@event.HomeId}, so its income limit can't be recorded.");

        facts.IncomeLimitPercentOfAmi = @event.PercentOfAmi;
        return MartenOps.Store(facts);
    }
}
