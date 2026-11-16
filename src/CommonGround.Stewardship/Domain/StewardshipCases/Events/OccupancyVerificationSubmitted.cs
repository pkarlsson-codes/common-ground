using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

public sealed record OccupancyVerificationSubmitted(
    string AggregateId,
    Guid VerificationId,
    bool IsPrimaryResident,
    bool SubletDisclosed,
    string? MajorChanges,
    DateOnly SubmittedDate) : DomainEvent(AggregateId);
