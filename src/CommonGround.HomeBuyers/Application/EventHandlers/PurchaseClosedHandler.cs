using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.Homeownership.Contracts.Events;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>Records on the household's application that the purchase it was selected for has closed.</summary>
public static class PurchaseClosedHandler
{
    /// <summary>Marks the application purchased, for purchases started from a home selection.</summary>
    public static async Task Handle(
        PurchaseClosed @event,
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

        application.RecordPurchaseClosed(@event.HomeId, @event.ClosingDate);
        session.UpdateAggregate(application);
    }
}
