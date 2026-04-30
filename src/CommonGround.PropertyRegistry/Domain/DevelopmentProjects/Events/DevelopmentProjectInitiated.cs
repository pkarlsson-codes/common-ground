using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.DevelopmentProjects.Events;

/// <summary>Raised when a new development project is initiated for a land parcel.</summary>
public sealed record DevelopmentProjectInitiated(
    string AggregateId,
    string LandParcelId,
    string Description,
    string InitiatedBy) : DomainEvent(AggregateId);
