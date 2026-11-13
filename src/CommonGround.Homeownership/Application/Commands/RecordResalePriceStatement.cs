using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records that a resale price statement was sent to a lease's homeowner.</summary>
/// <param name="GroundLeaseId">The lease.</param>
/// <param name="StatementDate">The letter's date, the day the price is worked out on, and the day it was sent.</param>
/// <param name="Figures">The figures the formula needs that the lease doesn't hold.</param>
/// <param name="Delivery">How it was sent.</param>
/// <param name="RecordedBy">The staff user recording it.</param>
public sealed record RecordResalePriceStatement(
    GroundLeaseId GroundLeaseId,
    DateOnly StatementDate,
    ResalePriceStatementFigures Figures,
    NoticeDelivery Delivery,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordResalePriceStatement"/>.</summary>
public static class RecordResalePriceStatementHandler
{
    /// <summary>Fetches the lease for writing and the trust's today.</summary>
    public static async Task<LeaseDecision> LoadAsync(
        RecordResalePriceStatement command,
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

        return new LeaseDecision(lease, today);
    }

    /// <summary>Records the statement on the lease.</summary>
    public static void Handle(RecordResalePriceStatement command, LeaseDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Lease.Decide(l => l.RecordResalePriceStatement(
            command.StatementDate,
            command.Figures,
            command.Delivery,
            decision.Today,
            command.RecordedBy));
    }
}
