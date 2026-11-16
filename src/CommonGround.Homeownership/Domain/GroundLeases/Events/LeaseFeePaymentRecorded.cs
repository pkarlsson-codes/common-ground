using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <param name="ReceivedDate">The day the trust received it.</param>
/// <param name="Reference">A check number or similar, if given.</param>
public sealed record LeaseFeePaymentRecorded(
    string AggregateId,
    string PaymentId,
    DateOnly ReceivedDate,
    Money Amount,
    PaymentMethod Method,
    string? Reference,
    string RecordedBy) : DomainEvent(AggregateId);
