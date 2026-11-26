using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Contracts.Events;
using Marten;
using Wolverine;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Loads and writes a home's <see cref="HomeAvailabilityView"/> for the handlers that keep it.</summary>
public static class HomeAvailabilityUpdate
{
    /// <summary>Loads the home's availability, if any news of its sale has arrived yet.</summary>
    public static Task<HomeAvailabilityView?> LoadAsync(IDocumentSession session, string homeId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        return session.LoadAsync<HomeAvailabilityView>(homeId, cancellationToken);
    }

    /// <summary>
    /// Records news on the home's availability, starting it when this is the first, and publishes
    /// <see cref="HomeSaleStageChanged"/> when the home's sale stage, the date it reached it, or the latest selection
    /// it accounts for changed. The view is stored with optimistic concurrency, so news handled at the same time is
    /// retried rather than overwritten, and each change gets its own sequence number.
    /// </summary>
    public static (IMartenOp, OutgoingMessages) Record(string homeId, HomeAvailabilityView? availability, Action<HomeAvailabilityView> record)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(record);

        var view = availability ?? new HomeAvailabilityView { Id = homeId };
        var before = (view.SaleStage, view.SaleStageSince, view.LatestSelectionId);
        record(view);

        var messages = new OutgoingMessages();
        if (before != (view.SaleStage, view.SaleStageSince, view.LatestSelectionId))
        {
            view.SaleStageSequence++;
            messages.Add(new HomeSaleStageChanged(
                homeId, view.SaleStage, view.SaleStageSince, view.SaleStageSequence, view.LatestSelectionId));
        }

        return (availability is null ? MartenOps.Insert(view) : MartenOps.Update(view), messages);
    }
}
