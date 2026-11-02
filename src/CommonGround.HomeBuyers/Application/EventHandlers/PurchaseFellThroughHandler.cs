using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.Homeownership.Contracts.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>What reacting to a purchase falling through reads and writes.</summary>
/// <param name="Application">The household's application stream.</param>
/// <param name="Selection">The completed selection that chose the household, unless its purchase has already fallen through.</param>
/// <param name="NextSelection">What a new selection for the home would start from.</param>
public sealed record FallenThroughPurchase(
    IEventStream<HomeownerApplication> Application,
    IEventStream<HomeSelection>? Selection,
    SelectionStart? NextSelection);

/// <summary>
/// When a purchase started from a home selection fails or is withdrawn, the household goes back on the
/// waiting list with its original approval date and the home goes straight back to selection.
/// </summary>
public static class PurchaseFellThroughHandler
{
    /// <summary>
    /// Fetches the application and, while it is still completed, the selection that chose it, both for
    /// writing, and snapshots the waiting list a new selection would start from. A purchase started
    /// directly has no application.
    /// </summary>
    public static async Task<FallenThroughPurchase?> LoadAsync(
        PurchaseFellThrough @event,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);

        if (@event.ApplicationId is null)
            return null;

        var application = await session.Events
            .FetchForWriting<HomeownerApplication>(@event.ApplicationId, cancellationToken)
            .ConfigureAwait(false);

        var completedStatus = HomeSelectionStatus.Completed.DisplayName;
        var selectionId = await session.Query<HomeSelectionView>()
            .Where(v => v.HomeId == @event.HomeId && v.Status == completedStatus && v.SelectedApplicationId == @event.ApplicationId)
            .Select(v => v.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (selectionId is null)
            return new FallenThroughPurchase(application, null, null);

        var selection = await session.Events
            .FetchForWriting<HomeSelection>(selectionId, cancellationToken)
            .ConfigureAwait(false);

        // Snapshotted while the household is still selected, so it isn't a candidate for the home
        // whose purchase it just fell through on.
        var nextSelection = await StartHomeSelectionHandler
            .LoadStartAsync(session, @event.HomeId, selectionId, cancellationToken)
            .ConfigureAwait(false);

        return new FallenThroughPurchase(application, selection, nextSelection);
    }

    /// <summary>
    /// Marks the selection that chose the household as fallen through, starts a new selection for the home
    /// (unless nobody is waiting), and returns the household's application to the waiting list. The new
    /// selection is started before the application returns, so the household isn't a candidate for the
    /// home whose purchase it just fell through on.
    /// </summary>
    public static IMartenOp Handle(PurchaseFellThrough @event, FallenThroughPurchase? purchase)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (purchase is null)
            return MartenOps.Nothing();

        IMartenOp nextSelection = MartenOps.Nothing();
        if (purchase.Selection is not null)
        {
            purchase.Selection.Decide(s => s.RecordPurchaseFellThrough(@event.Reason, @event.FellThroughDate));

            if (purchase.NextSelection is { Candidates.Count: > 0 } next)
            {
                nextSelection = StartHomeSelectionHandler.Start(
                    HomeSelectionId.Generate(), @event.HomeId, next, UserId.Create(@event.RecordedBy));
            }
        }

        purchase.Application.Decide(a => a.ReturnToWaitingList(@event.HomeId, @event.Reason, @event.FellThroughDate));

        return nextSelection;
    }
}
