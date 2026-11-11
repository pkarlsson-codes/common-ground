using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
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
    /// <summary>Fetches the lease for writing and the trust's today.</summary>
    public static async Task<LeaseFeePaymentDecision> LoadAsync(
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

        return new LeaseFeePaymentDecision(lease, today);
    }

    /// <summary>Records the notice on the lease.</summary>
    public static void Handle(RecordLeaseFeeNotice command, LeaseFeePaymentDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Lease.Decide(l => l.RecordLeaseFeeNotice(
            command.NoticeId,
            command.Level,
            command.SentOn,
            command.Delivery,
            decision.Today,
            command.RecordedBy));
    }
}
