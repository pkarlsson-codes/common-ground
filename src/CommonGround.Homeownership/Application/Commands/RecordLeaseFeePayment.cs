using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records a lease fee payment the trust received on a lease.</summary>
/// <param name="GroundLeaseId">The lease the payment is for.</param>
/// <param name="PaymentId">The new payment's ID, chosen by the caller.</param>
/// <param name="ReceivedDate">The day the trust received it.</param>
/// <param name="Amount">The amount received.</param>
/// <param name="Method">How it was paid.</param>
/// <param name="Reference">A check number or similar, if given.</param>
/// <param name="RecordedBy">The staff user recording it.</param>
public sealed record RecordLeaseFeePayment(
    GroundLeaseId GroundLeaseId,
    LeaseFeePaymentId PaymentId,
    DateOnly ReceivedDate,
    Money Amount,
    PaymentMethod Method,
    string? Reference,
    UserId RecordedBy);

/// <summary>What recording a payment or a notice on a lease decides from.</summary>
/// <param name="Lease">The lease stream, fetched for writing.</param>
/// <param name="Today">The trust's today.</param>
public sealed record LeaseFeePaymentDecision(IEventStream<GroundLease> Lease, DateOnly Today);

/// <summary>Handles <see cref="RecordLeaseFeePayment"/>.</summary>
public static class RecordLeaseFeePaymentHandler
{
    /// <summary>Fetches the lease for writing and the trust's today.</summary>
    public static async Task<LeaseFeePaymentDecision> LoadAsync(
        RecordLeaseFeePayment command,
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

    /// <summary>Records the payment on the lease.</summary>
    public static void Handle(RecordLeaseFeePayment command, LeaseFeePaymentDecision decision)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(decision);

        decision.Lease.Decide(l => l.RecordLeaseFeePayment(
            command.PaymentId,
            command.ReceivedDate,
            command.Amount,
            command.Method,
            command.Reference,
            decision.Today,
            command.RecordedBy));
    }
}
