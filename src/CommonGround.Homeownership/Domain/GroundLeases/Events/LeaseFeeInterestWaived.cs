using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <param name="WaivedOn">The day it was waived, the trust's today.</param>
public sealed record LeaseFeeInterestWaived(
    string AggregateId,
    string WaiverId,
    DateOnly WaivedOn,
    Money Amount,
    string Reason,
    string WaivedBy) : DomainEvent(AggregateId);
