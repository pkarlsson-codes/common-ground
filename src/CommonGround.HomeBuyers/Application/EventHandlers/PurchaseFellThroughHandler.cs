using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.Homeownership.Contracts.Events;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>
/// When a purchase started from a home selection fails or is withdrawn, the household goes back on the waiting list
/// with its original approval date. The home becomes available again through PropertyRegistry; no household is
/// chosen for it until staff choose one.
/// </summary>
public static class PurchaseFellThroughHandler
{
    /// <summary>Fetches the household's application for writing. A purchase started directly has no application.</summary>
    public static async Task<IEventStream<HomeownerApplication>?> LoadAsync(
        PurchaseFellThrough @event,
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

    /// <summary>Returns the household's application to the waiting list.</summary>
    public static void Handle(PurchaseFellThrough @event, IEventStream<HomeownerApplication>? application)
    {
        ArgumentNullException.ThrowIfNull(@event);

        application?.Decide(a => a.ReturnToWaitingList(@event.HomeId, @event.Reason, @event.FellThroughDate));
    }
}
