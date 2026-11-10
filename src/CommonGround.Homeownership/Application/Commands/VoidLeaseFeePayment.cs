using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Voids a lease fee payment recorded by mistake.</summary>
/// <param name="GroundLeaseId">The lease the payment is on.</param>
/// <param name="PaymentId">The payment to void.</param>
/// <param name="Reason">Why it is voided.</param>
/// <param name="VoidedBy">The staff user voiding it.</param>
public sealed record VoidLeaseFeePayment(
    GroundLeaseId GroundLeaseId,
    LeaseFeePaymentId PaymentId,
    string Reason,
    UserId VoidedBy);

/// <summary>Handles <see cref="VoidLeaseFeePayment"/>.</summary>
public static class VoidLeaseFeePaymentHandler
{
    /// <summary>Voids the payment on the lease.</summary>
    public static LeaseFeePaymentVoided Handle(VoidLeaseFeePayment command, [WriteAggregate] GroundLease lease)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(lease);

        return lease.VoidLeaseFeePayment(command.PaymentId, command.Reason, command.VoidedBy);
    }
}
