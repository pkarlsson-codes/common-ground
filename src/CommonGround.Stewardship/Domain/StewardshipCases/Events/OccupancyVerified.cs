using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

public sealed record OccupancyVerified(
    string AggregateId,
    Guid VerificationId,
    DateOnly VerifiedDate,
    string VerifiedBy) : DomainEvent(AggregateId);
