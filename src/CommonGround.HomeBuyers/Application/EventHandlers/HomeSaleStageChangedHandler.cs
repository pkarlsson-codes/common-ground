using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.PropertyRegistry.Contracts.Events;
using Marten;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>
/// Keeps <see cref="HomeFactsView"/>'s sale stage in step with PropertyRegistry's HomeSaleStageChanged integration
/// event, so households are chosen only for homes that are offered.
/// </summary>
public static class HomeSaleStageChangedHandler
{
    /// <summary>Loads the home's facts.</summary>
    public static Task<HomeFactsView?> LoadAsync(HomeSaleStageChanged @event, IDocumentSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);

        return session.LoadAsync<HomeFactsView>(@event.HomeId, cancellationToken);
    }

    /// <summary>
    /// Records the home's new sale stage, unless a later one has already arrived. Throws when HomeBuyers has no facts
    /// for the home, so the message fails and lands in the dead letter queue instead of leaving the stage stale.
    /// </summary>
    public static IMartenOp Handle(HomeSaleStageChanged @event, HomeFactsView? facts)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (facts is null)
            throw new InvalidOperationException($"HomeBuyers has no facts for home {@event.HomeId}, so its sale stage can't be recorded.");

        if (@event.Sequence <= facts.SaleStageSequence)
            return MartenOps.Nothing();

        facts.SaleStage = @event.SaleStage;
        facts.SaleStageSince = @event.Since;
        facts.SaleStageSequence = @event.Sequence;
        facts.SaleStageSelectionId = @event.LatestSelectionId;
        return MartenOps.Update(facts);
    }
}
