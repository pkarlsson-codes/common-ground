using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.ImprovementRequests.Events;

/// <summary>Raised when the trust refuses the work. The reason goes in the denial letter.</summary>
/// <param name="DeniedOn">The day the trust refused it.</param>
public sealed record ImprovementRequestDenied(
    string AggregateId,
    string GroundLeaseId,
    DateOnly DeniedOn,
    string Reason,
    string DeniedBy) : DomainEvent(AggregateId);
