using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Insurance;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns a single ground lease by ID.</summary>
public sealed record GetGroundLease(GroundLeaseId GroundLeaseId);

/// <summary>
/// A ground lease, the trust date it was recorded on (for a lease that predates CommonGround, later than its start
/// date), what it owes in lease fees as of the trust's today, the late notice it needs now, if any, and where its
/// last notice of default stands, if one was sent, and when its next resale price statement is due (null for a lease
/// off the yearly cycle: ended, or with a resale in progress), the insurance it requires as of the trust's today,
/// and where its insurance stands from its certificates.
/// </summary>
public sealed record GroundLeaseDetails(
    GroundLeaseView Lease,
    DateOnly RecordedOn,
    LeaseFeeStatement LeaseFees,
    LeaseFeeNoticeLevel? NoticeDue,
    DefaultCure? DefaultCure,
    DateOnly? NextResalePriceStatementDueOn,
    InsuranceRequirements InsuranceRequired,
    InsuranceCover InsuranceCover);

/// <summary>Handles <see cref="GetGroundLease"/>.</summary>
public static class GetGroundLeaseHandler
{
    /// <summary>Loads the ground lease view by ID, throwing if not found.</summary>
    public static async Task<GroundLeaseDetails> Handle(
        GetGroundLease request,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var view = await session.LoadAsync<GroundLeaseView>(request.GroundLeaseId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.GroundLeaseId.Value);

        var recordedOn = await trustClock.ToTrustDateAsync(session.TenantId, view.CreatedAt, cancellationToken).ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        var policy = await GetLeaseFeeNoticePolicyHandler.LoadAsync(session, cancellationToken).ConfigureAwait(false);
        var statement = LeaseFeeStatements.For(view, today);

        return new GroundLeaseDetails(
            view,
            recordedOn,
            statement,
            LeaseFeeStatements.NoticeDue(view, statement, policy.Schedule),
            LeaseFeeStatements.DefaultCureOf(view, today),
            ResalePriceStatementSchedule.For(view, null)?.NextDueOn,
            InsuranceRequirementsOf(view).InEffectOn(today),
            LeaseInsurance.CoverOf(view));
    }

    /// <summary>The insurance the lease has required, from its read model.</summary>
    public static InsuranceRequirementsHistory InsuranceRequirementsOf(GroundLeaseView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        return InsuranceRequirementsHistory.From(view.InsuranceRequirements.Select(r => InsuranceRequirementsChange.Create(
            r.EffectiveFrom,
            InsuranceRequirements.Create(
                r.ReplacementValue is { } replacementValue ? Money.Create(replacementValue) : null,
                r.LiabilityPerOccurrence is { } liability ? Money.Create(liability) : null))));
    }
}
