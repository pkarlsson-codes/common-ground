using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.MortgageRequests.Events;

/// <param name="WithdrawnOn">The day the homeowner withdrew it.</param>
public sealed record MortgageRequestWithdrawn(
    string AggregateId,
    string GroundLeaseId,
    DateOnly WithdrawnOn,
    string Reason,
    string WithdrawnBy) : DomainEvent(AggregateId);
