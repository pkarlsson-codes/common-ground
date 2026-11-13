using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Application.WorkItems;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Application.WorkItems;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>
/// Returns Homeownership's work items: one for every open purchase, every lease with a late notice due, and every
/// active lease with an uncured notice of default, unless staff are already working with the homeowner in a
/// foreclosure prevention case.
/// </summary>
/// <param name="Today">The trust's today, against which upcoming closings are judged.</param>
/// <param name="LeasesWithOpenPreventionCase">The leases with an unresolved foreclosure prevention case, from
/// Stewardship. Their uncured default is left out: the case is the work.</param>
public sealed record GetHomeownershipWorkItems(DateOnly Today, IReadOnlySet<string> LeasesWithOpenPreventionCase);

/// <summary>Handles <see cref="GetHomeownershipWorkItems"/>.</summary>
public static class GetHomeownershipWorkItemsHandler
{
    private static readonly string[] TerminalPhaseNames =
    [
        WorkflowPhase.Closed.DisplayName,
        WorkflowPhase.Failed.DisplayName,
        WorkflowPhase.Withdrawn.DisplayName,
    ];

    /// <summary>Loads the open purchases and derives their work items.</summary>
    public static async Task<WorkItemList> Handle(
        GetHomeownershipWorkItems request,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var zone = await trustClock.GetTimeZoneAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        var workflows = await session.Query<PurchaseWorkflowView>()
            .Where(v => !TerminalPhaseNames.Contains(v.Phase))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var listItems = (await session
            .LoadManyAsync<PurchaseWorkflowListItemView>(cancellationToken, workflows.Select(w => w.Id))
            .ConfigureAwait(false))
            .ToDictionary(v => v.Id);
        var labels = (await session
            .LoadManyAsync<HomeLabelView>(cancellationToken, workflows.Select(w => w.HomeId).Distinct())
            .ConfigureAwait(false))
            .ToDictionary(v => v.Id, v => v.Label);

        var policy = await GetLeaseFeeNoticePolicyHandler.LoadAsync(session, cancellationToken).ConfigureAwait(false);
        var balances = await LeaseFeeBalances.LoadAsync(session, request.Today, policy.Schedule, cancellationToken).ConfigureAwait(false);
        var notices = balances.Where(b => b.NoticeDue is not null);
        var uncured = balances.Where(b => b.Lease.Status == GroundLeaseStatus.Active.DisplayName
            && b.DefaultCure?.Status == DefaultCureStatus.Uncured
            && !request.LeasesWithOpenPreventionCase.Contains(b.Lease.Id));

        return new WorkItemList([
            .. workflows.Select(w => HomeownershipWorkItems.ForPurchase(
                w,
                listItems.GetValueOrDefault(w.Id)?.StepEnteredAt ?? w.CreatedAt,
                labels.GetValueOrDefault(w.HomeId),
                request.Today,
                zone)),
            .. notices.Select(b => HomeownershipWorkItems.ForLeaseFeeNotice(b, policy.Schedule)),
            .. uncured.Select(HomeownershipWorkItems.ForUncuredDefault),
        ]);
    }
}
