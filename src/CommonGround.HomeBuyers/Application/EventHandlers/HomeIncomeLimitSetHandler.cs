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

    /// <summary>Records the home's new income limit. A home HomeBuyers has no facts for is left alone.</summary>
    public static IMartenOp Handle(HomeIncomeLimitSet @event, HomeFactsView? facts)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (facts is null)
            return MartenOps.Nothing();

        facts.IncomeLimitPercentOfAmi = @event.PercentOfAmi;
        return MartenOps.Store(facts);
    }
}
