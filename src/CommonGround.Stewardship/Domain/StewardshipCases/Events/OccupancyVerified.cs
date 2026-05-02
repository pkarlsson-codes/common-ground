using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <summary>Raised when an occupancy verification is confirmed as verified.</summary>
/// <param name="AggregateId">The stewardship case identifier.</param>
/// <param name="VerificationId">The verification being verified.</param>
/// <param name="VerifiedDate">The date of the verification decision.</param>
/// <param name="VerifiedBy">The staff member who verified the occupancy.</param>
public sealed record OccupancyVerified(
    string AggregateId,
    Guid VerificationId,
    DateOnly VerifiedDate,
    string VerifiedBy) : DomainEvent(AggregateId);
