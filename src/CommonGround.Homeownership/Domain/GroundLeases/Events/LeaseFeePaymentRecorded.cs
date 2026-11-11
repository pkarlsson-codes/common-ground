using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when the trust records a lease fee payment it received on a lease.</summary>
/// <param name="AggregateId">The ground lease.</param>
/// <param name="PaymentId">The new payment's ID.</param>
/// <param name="ReceivedDate">The day the trust received it.</param>
/// <param name="Amount">The amount received.</param>
/// <param name="Method">How it was paid.</param>
/// <param name="Reference">A check number or similar, if given.</param>
/// <param name="RecordedBy">The staff user who recorded it.</param>
public sealed record LeaseFeePaymentRecorded(
    string AggregateId,
    string PaymentId,
    DateOnly ReceivedDate,
    Money Amount,
    PaymentMethod Method,
    string? Reference,
    string RecordedBy) : DomainEvent(AggregateId);
