using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.Households.Events;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using CommonGround.SharedKernel.Specifications;
using Marten;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>
/// When a household's income, size or accessibility need changes, its open application takes the household's figures
/// as they are now, so staff update the household and nothing else. Each change reads the household's current state
/// rather than the event's, so changes that arrive late or out of order still leave the application matching the
/// household, and a repeat changes nothing.
/// </summary>
public static class HouseholdChangedHandler
{
    /// <summary>The application follows the household's new circumstances.</summary>
    public static Task Handle(
        HouseholdCircumstancesUpdated @event, IDocumentSession session, ITrustClock trustClock, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return FollowAsync(@event.AggregateId, session, trustClock, cancellationToken);
    }

    /// <summary>The application follows the household's new size.</summary>
    public static Task Handle(
        HouseholdMemberAdded @event, IDocumentSession session, ITrustClock trustClock, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return FollowAsync(@event.AggregateId, session, trustClock, cancellationToken);
    }

    /// <summary>The application follows the household's new size.</summary>
    public static Task Handle(
        HouseholdMemberRemoved @event, IDocumentSession session, ITrustClock trustClock, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return FollowAsync(@event.AggregateId, session, trustClock, cancellationToken);
    }

    private static async Task FollowAsync(
        string householdId, IDocumentSession session, ITrustClock trustClock, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var open = await session.Query<HomeownerApplicationListItemView>()
            .Where(v => v.HouseholdId == householdId)
            .Matching(ApplicationOpen<HomeownerApplicationListItemView>.Instance)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (open is null)
            return;

        var household = await session
            .LoadAggregateAsync<Household>(HouseholdId.Create(householdId), cancellationToken)
            .ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        var application = await session.Events
            .FetchForWriting<HomeownerApplication>(open.Id, cancellationToken)
            .ConfigureAwait(false);
        application.Decide(a => a.FollowHousehold(household.Declare(), today));
    }
}
