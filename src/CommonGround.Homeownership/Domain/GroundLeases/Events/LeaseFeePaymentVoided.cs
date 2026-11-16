using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when a lease fee payment recorded by mistake is voided, so it no longer counts toward the balance.</summary>
public sealed record LeaseFeePaymentVoided(
    string AggregateId,
    string PaymentId,
    string Reason,
    string VoidedBy) : DomainEvent(AggregateId);
