using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.DevelopmentPlans.Events;

/// <summary>Raised when a new development plan is initiated for a land parcel.</summary>
public sealed record DevelopmentPlanInitiated(
    string AggregateId,
    string LandParcelId,
    string Description,
    string InitiatedBy) : DomainEvent(AggregateId);
