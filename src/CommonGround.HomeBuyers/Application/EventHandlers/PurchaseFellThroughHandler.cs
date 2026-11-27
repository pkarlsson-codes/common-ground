using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.Homeownership.Contracts.Events;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>The returning application, fetched for writing, its household as it is now, and the trust's today.</summary>
public sealed record PurchaseFellThroughDecision(
    IEventStream<HomeownerApplication> Application,
    Household Household,
    DateOnly Today);

/// <summary>
/// When a purchase started from a home selection fails or is withdrawn, the household goes back on the waiting list
/// with its original approval date. The application kept the figures it was chosen on while selected, so it then
/// follows the household: a change made during the purchase marks it for staff to confirm the household still
/// qualifies. The home becomes available again through PropertyRegistry; no household is chosen for it until staff
/// choose one.
/// </summary>
public static class PurchaseFellThroughHandler
{
    /// <summary>
    /// Fetches the household's application for writing, with its household and the trust's today. A purchase started
    /// directly has no application.
    /// </summary>
    public static async Task<PurchaseFellThroughDecision?> LoadAsync(
        PurchaseFellThrough @event,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        if (@event.ApplicationId is null)
            return null;

        var application = await session.Events
            .FetchForWriting<HomeownerApplication>(@event.ApplicationId, cancellationToken)
            .ConfigureAwait(false);
        var household = await session
            .LoadAggregateAsync<Household>(application.Aggregate!.HouseholdId, cancellationToken)
            .ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new PurchaseFellThroughDecision(application, household, today);
    }

    /// <summary>Returns the household's application to the waiting list, then has it take the household's current figures.</summary>
    public static void Handle(PurchaseFellThrough @event, PurchaseFellThroughDecision? decision)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (decision is null)
            return;

        decision.Application.Decide(a => a.ReturnToWaitingList(@event.HomeId, @event.Reason, @event.FellThroughDate));
        decision.Application.Decide(a => a.FollowHousehold(decision.Household.Declare(), decision.Today));
    }
}
