using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.ImprovementRequests.Events;

/// <summary>Raised when the trust refuses the work. The reason goes in the denial letter.</summary>
/// <param name="AggregateId">The improvement request ID.</param>
/// <param name="GroundLeaseId">The homeowner's lease.</param>
/// <param name="DeniedOn">The day the trust refused it.</param>
/// <param name="Reason">Why the trust refused it.</param>
/// <param name="DeniedBy">The staff user who recorded it.</param>
public sealed record ImprovementRequestDenied(
    string AggregateId,
    string GroundLeaseId,
    DateOnly DeniedOn,
    string Reason,
    string DeniedBy) : DomainEvent(AggregateId);
