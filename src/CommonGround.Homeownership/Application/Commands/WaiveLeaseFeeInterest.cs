using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Waives late-fee interest owed on a lease today.</summary>
/// <param name="GroundLeaseId">The lease.</param>
/// <param name="WaiverId">The new waiver's ID, chosen by the caller.</param>
/// <param name="Amount">How much interest to waive.</param>
/// <param name="Reason">Why.</param>
/// <param name="WaivedBy">The staff user waiving it.</param>
public sealed record WaiveLeaseFeeInterest(
    GroundLeaseId GroundLeaseId,
    LeaseFeeInterestWaiverId WaiverId,
    decimal Amount,
    string Reason,
    UserId WaivedBy);

/// <summary>Handles <see cref="WaiveLeaseFeeInterest"/>.</summary>
public static class WaiveLeaseFeeInterestHandler
{
    /// <summary>Fetches the lease for writing and the trust's today.</summary>
    public static async Task<LeaseFeePaymentDecision> LoadAsync(
        WaiveLeaseFeeInterest command,
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

    /// <summary>Waives the interest on the lease.</summary>
    public static void Handle(WaiveLeaseFeeInterest command, LeaseFeePaymentDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Lease.Decide(l => l.WaiveLeaseFeeInterest(
            command.WaiverId,
            command.Amount,
            command.Reason,
            decision.Today,
            command.WaivedBy));
    }
}
