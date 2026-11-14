using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Application.WorkItems;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.ImprovementRequests;
using CommonGround.Homeownership.Domain.MortgageRequests;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Application.WorkItems;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Specifications;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>
/// Returns Homeownership's work items: one for every open purchase, every lease with a late notice due, every
/// active lease with an uncured notice of default (unless staff are already working with the homeowner in a
/// foreclosure prevention case), every pending mortgage request, every approved loan past its expected closing, and
/// every lease whose yearly resale price statement is due, every active lease whose insurance certificate is due, and
/// every active lease whose current insurance certificate falls short.
/// </summary>
/// <param name="Today">The trust's today, against which upcoming closings are judged.</param>
/// <param name="LeasesWithOpenPreventionCase">The leases with an unresolved foreclosure prevention case, from
/// Stewardship. Their uncured default is left out: the case is the work.</param>
public sealed record GetHomeownershipWorkItems(DateOnly Today, IReadOnlySet<string> LeasesWithOpenPreventionCase);

/// <summary>Handles <see cref="GetHomeownershipWorkItems"/>.</summary>
public static class GetHomeownershipWorkItemsHandler
{

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
            .Matching(PurchaseWorkflowInProgress<PurchaseWorkflowView>.Instance)
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

        var openRequestStatuses = new[] { MortgageRequestStatus.Pending.DisplayName, MortgageRequestStatus.Approved.DisplayName };
        var requests = await session.Query<MortgageRequestView>()
            .Where(v => openRequestStatuses.Contains(v.Status))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var requestLabels = (await session
            .LoadManyAsync<HomeLabelView>(cancellationToken, requests.Select(r => r.HomeId).Distinct())
            .ConfigureAwait(false))
            .ToDictionary(v => v.Id, v => v.Label);

        var waitingStatus = ImprovementRequestStatus.Waiting.DisplayName;
        var improvementRequests = await session.Query<ImprovementRequestView>()
            .Where(v => v.Status == waitingStatus)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var improvementLabels = (await session
            .LoadManyAsync<HomeLabelView>(cancellationToken, improvementRequests.Select(r => r.HomeId).Distinct())
            .ConfigureAwait(false))
            .ToDictionary(v => v.Id, v => v.Label);

        var statements = (await ResalePriceStatementSchedule.LoadAllAsync(session, cancellationToken).ConfigureAwait(false))
            .Where(s => s.NextDueOn <= request.Today);

        var insurance = await LeaseInsurance.LoadActiveAsync(session, cancellationToken).ConfigureAwait(false);

        return new WorkItemList([
            .. workflows.Select(w => HomeownershipWorkItems.ForPurchase(
                w,
                listItems.GetValueOrDefault(w.Id)?.StepEnteredAt ?? w.CreatedAt,
                labels.GetValueOrDefault(w.HomeId),
                request.Today,
                zone)),
            .. notices.Select(b => HomeownershipWorkItems.ForLeaseFeeNotice(b, policy.Schedule)),
            .. uncured.Select(HomeownershipWorkItems.ForUncuredDefault),
            .. requests
                .Select(r => HomeownershipWorkItems.ForMortgageRequest(r, requestLabels.GetValueOrDefault(r.HomeId), request.Today))
                .OfType<WorkItem>(),
            .. improvementRequests
                .Select(r => HomeownershipWorkItems.ForImprovementRequest(r, improvementLabels.GetValueOrDefault(r.HomeId)))
                .OfType<WorkItem>(),
            .. statements.Select(HomeownershipWorkItems.ForResalePriceStatement),
            .. insurance
                .Where(i => i.Cover.IsCertificateDueOn(request.Today))
                .Select(i => HomeownershipWorkItems.ForInsuranceCertificateDue(i, request.Today)),
            .. insurance.Where(i => i.Cover.IsShort).Select(HomeownershipWorkItems.ForInsuranceShortOfLease),
        ]);
    }
}
