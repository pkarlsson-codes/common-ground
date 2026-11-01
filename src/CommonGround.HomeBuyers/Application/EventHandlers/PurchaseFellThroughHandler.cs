using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.Homeownership.Contracts.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>
/// When a purchase started from a home selection fails or is withdrawn, the household goes back on the
/// waiting list with its original approval date and the home goes straight back to selection.
/// </summary>
public static class PurchaseFellThroughHandler
{
    /// <summary>
    /// Marks the selection that chose the household as fallen through, starts a new selection for the home
    /// (unless nobody is waiting), and returns the household's application to the waiting list. The new
    /// selection is started before the application returns, so the household isn't a candidate for the
    /// home whose purchase it just fell through on.
    /// </summary>
    public static async Task Handle(
        PurchaseFellThrough @event,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (@event.ApplicationId is null)
            return;

        var application = await session
            .LoadAggregateAsync<HomeownerApplication>(HomeownerApplicationId.Create(@event.ApplicationId), cancellationToken)
            .ConfigureAwait(false);
        if (application.Phase != ApplicationPhase.Selected)
            return;

        var completedStatus = HomeSelectionStatus.Completed.DisplayName;
        var selectionId = await session.Query<HomeSelectionView>()
            .Where(v => v.HomeId == @event.HomeId && v.Status == completedStatus && v.SelectedApplicationId == @event.ApplicationId)
            .Select(v => v.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (selectionId is not null)
        {
            var selection = await session
                .LoadAggregateAsync<HomeSelection>(HomeSelectionId.Create(selectionId), cancellationToken)
                .ConfigureAwait(false);
            selection.RecordPurchaseFellThrough(@event.Reason, @event.FellThroughDate);
            session.UpdateAggregate(selection);

            var approvedPhase = ApplicationPhase.Approved.DisplayName;
            var anyoneWaiting = await session.Query<HomeownerApplicationView>()
                .AnyAsync(v => v.Phase == approvedPhase, cancellationToken)
                .ConfigureAwait(false);
            if (anyoneWaiting)
            {
                await StartHomeSelectionHandler
                    .StartAsync(session, @event.HomeId, UserId.Create(@event.RecordedBy), selectionId, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        application.ReturnToWaitingList(@event.HomeId, @event.Reason, @event.FellThroughDate);
        session.UpdateAggregate(application);
    }
}
