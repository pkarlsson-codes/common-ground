using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Application.WorkItems;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.SharedKernel.Application.WorkItems;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.HomeBuyers.Application.Queries;

/// <summary>Returns HomeBuyers' work items: open applications, their households' readiness, open selections, and missing or out-of-date income limits.</summary>
/// <param name="Today">The trust's today, against which upcoming dates are judged.</param>
public sealed record GetHomeBuyersWorkItems(DateOnly Today);

/// <summary>Handles <see cref="GetHomeBuyersWorkItems"/>.</summary>
public static class GetHomeBuyersWorkItemsHandler
{
    /// <summary>Loads the open applications and selections and derives their work items.</summary>
    public static async Task<WorkItemList> Handle(
        GetHomeBuyersWorkItems request,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var zone = await trustClock.GetTimeZoneAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        var submitted = ApplicationPhase.Submitted.DisplayName;
        var approved = ApplicationPhase.Approved.DisplayName;
        var applications = await session.Query<HomeownerApplicationListItemView>()
            .Where(v => v.Phase == submitted || v.Phase == approved)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var households = (await session
            .LoadManyAsync<HouseholdListItemView>(cancellationToken, applications.Select(a => a.HouseholdId).Distinct())
            .ConfigureAwait(false))
            .ToDictionary(h => h.Id);

        var open = HomeSelectionStatus.Open.DisplayName;
        var selections = await session.Query<HomeSelectionView>()
            .Where(v => v.Status == open)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var homes = (await session
            .LoadManyAsync<HomeFactsView>(cancellationToken, selections.Select(s => s.HomeId).Distinct())
            .ConfigureAwait(false))
            .ToDictionary(h => h.Id);

        var limitedHomes = await session.Query<HomeFactsView>()
            .Where(h => h.IncomeLimitPercentOfAmi != null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var schedules = await session.Query<IncomeLimitScheduleView>()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new WorkItemList([
            .. applications.SelectMany(a =>
                HomeBuyersWorkItems.ForApplication(a, households.GetValueOrDefault(a.HouseholdId), request.Today, zone)),
            .. selections.Select(s => HomeBuyersWorkItems.ForSelection(s, homes.GetValueOrDefault(s.HomeId)?.Label, zone)),
            .. HomeBuyersWorkItems.ForIncomeLimits(limitedHomes, schedules, request.Today),
        ]);
    }
}
