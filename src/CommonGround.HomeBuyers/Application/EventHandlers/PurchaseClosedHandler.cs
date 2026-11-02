using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.Homeownership.Contracts.Events;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>Records on the household's application that the purchase it was selected for has closed.</summary>
public static class PurchaseClosedHandler
{
    /// <summary>Fetches the application's stream for writing; a purchase started directly has none.</summary>
    public static async Task<IEventStream<HomeownerApplication>?> LoadAsync(
        PurchaseClosed @event,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);

        if (@event.ApplicationId is null)
            return null;

        return await session.Events
            .FetchForWriting<HomeownerApplication>(@event.ApplicationId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Marks the application purchased, for purchases started from a home selection.</summary>
    public static void Handle(PurchaseClosed @event, IEventStream<HomeownerApplication>? application)
    {
        ArgumentNullException.ThrowIfNull(@event);

        application?.Decide(a => a.RecordPurchaseClosed(@event.HomeId, @event.ClosingDate));
    }
}
