using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when staff waive late-fee interest owed on a lease.</summary>
/// <param name="AggregateId">The ground lease ID.</param>
/// <param name="WaiverId">The waiver's ID.</param>
/// <param name="WaivedOn">The day it was waived, the trust's today.</param>
/// <param name="Amount">How much interest was waived.</param>
/// <param name="Reason">Why.</param>
/// <param name="WaivedBy">The staff user who waived it.</param>
public sealed record LeaseFeeInterestWaived(
    string AggregateId,
    string WaiverId,
    DateOnly WaivedOn,
    decimal Amount,
    string Reason,
    string WaivedBy) : DomainEvent(AggregateId);
