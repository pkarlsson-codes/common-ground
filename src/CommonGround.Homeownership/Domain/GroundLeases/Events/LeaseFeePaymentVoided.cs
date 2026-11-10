using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when a lease fee payment recorded by mistake is voided, so it no longer counts toward the balance.</summary>
/// <param name="AggregateId">The ground lease.</param>
/// <param name="PaymentId">The voided payment.</param>
/// <param name="Reason">Why it was voided.</param>
/// <param name="VoidedBy">The staff user who voided it.</param>
public sealed record LeaseFeePaymentVoided(
    string AggregateId,
    string PaymentId,
    string Reason,
    string VoidedBy) : DomainEvent(AggregateId);
