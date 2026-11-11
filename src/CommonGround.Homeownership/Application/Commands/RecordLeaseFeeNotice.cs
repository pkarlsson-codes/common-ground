using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using CommonGround.Homeownership.Application.Queries;
using JasperFx.Events;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that a late lease fee notice was sent on a lease.</summary>
/// <param name="GroundLeaseId">The lease the notice is about.</param>
/// <param name="NoticeId">The new notice's ID, chosen by the caller.</param>
/// <param name="Level">Which notice it was.</param>
/// <param name="SentOn">The day it was sent.</param>
/// <param name="Delivery">How it was sent.</param>
/// <param name="RecordedBy">The staff user recording it.</param>
public sealed record RecordLeaseFeeNotice(
    GroundLeaseId GroundLeaseId,
    LeaseFeeNoticeId NoticeId,
    LeaseFeeNoticeLevel Level,
    DateOnly SentOn,
    NoticeDelivery Delivery,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordLeaseFeeNotice"/>.</summary>
public static class RecordLeaseFeeNoticeHandler
{
    /// <summary>Fetches the lease for writing, the trust's today, and the cure period a notice of default gives.</summary>
    public static async Task<LeaseFeeNoticeDecision> LoadAsync(
        RecordLeaseFeeNotice command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var lease = await session.Events
            .FetchForWriting<GroundLease>(command.GroundLeaseId.Value, cancellationToken)
            .ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);
        var policy = await GetLeaseFeeNoticePolicyHandler.LoadAsync(session, cancellationToken).ConfigureAwait(false);

        return new LeaseFeeNoticeDecision(lease, today, policy.CurePeriod);
    }

    /// <summary>Records the notice on the lease.</summary>
    public static void Handle(RecordLeaseFeeNotice command, LeaseFeeNoticeDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Lease.Decide(l => l.RecordLeaseFeeNotice(
            command.NoticeId,
            command.Level,
            command.SentOn,
            command.Delivery,
            decision.CurePeriod,
            decision.Today,
            command.RecordedBy));
    }
}

/// <summary>What recording a late notice decides with.</summary>
/// <param name="Lease">The lease, fetched for writing.</param>
/// <param name="Today">The trust's today.</param>
/// <param name="CurePeriod">The trust's cure period for a notice of default.</param>
public sealed record LeaseFeeNoticeDecision(IEventStream<GroundLease> Lease, DateOnly Today, DefaultCurePeriod CurePeriod);
