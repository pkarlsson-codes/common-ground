using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.ImprovementRequests.Events;

/// <summary>Raised when the homeowner drops the work before it is recorded finished.</summary>
/// <param name="AggregateId">The improvement request ID.</param>
/// <param name="GroundLeaseId">The homeowner's lease.</param>
/// <param name="WithdrawnOn">The day the homeowner withdrew it.</param>
/// <param name="Reason">Why the homeowner withdrew it.</param>
/// <param name="WithdrawnBy">The staff user who recorded it.</param>
public sealed record ImprovementRequestWithdrawn(
    string AggregateId,
    string GroundLeaseId,
    DateOnly WithdrawnOn,
    string Reason,
    string WithdrawnBy) : DomainEvent(AggregateId);
