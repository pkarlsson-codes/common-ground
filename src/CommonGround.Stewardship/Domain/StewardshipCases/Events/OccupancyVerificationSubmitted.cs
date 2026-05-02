using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <summary>Raised when a homeowner submits their occupancy survey.</summary>
/// <param name="AggregateId">The stewardship case identifier.</param>
/// <param name="VerificationId">The verification being submitted.</param>
/// <param name="IsPrimaryResident">Whether the homeowner confirmed primary residency.</param>
/// <param name="SubletDisclosed">Whether any sublet arrangement was disclosed.</param>
/// <param name="MajorChanges">Any major changes reported.</param>
/// <param name="SubmittedDate">The date of submission.</param>
public sealed record OccupancyVerificationSubmitted(
    string AggregateId,
    Guid VerificationId,
    bool IsPrimaryResident,
    bool SubletDisclosed,
    string? MajorChanges,
    DateOnly SubmittedDate) : DomainEvent(AggregateId);
