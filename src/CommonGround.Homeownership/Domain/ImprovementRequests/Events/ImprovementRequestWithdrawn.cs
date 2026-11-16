using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.ImprovementRequests.Events;

/// <param name="WithdrawnOn">The day the homeowner withdrew it.</param>
public sealed record ImprovementRequestWithdrawn(
    string AggregateId,
    string GroundLeaseId,
    DateOnly WithdrawnOn,
    string Reason,
    string WithdrawnBy) : DomainEvent(AggregateId);
