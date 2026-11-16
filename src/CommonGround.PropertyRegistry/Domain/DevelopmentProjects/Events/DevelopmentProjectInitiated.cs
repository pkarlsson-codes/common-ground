using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.DevelopmentProjects.Events;

public sealed record DevelopmentProjectInitiated(
    string AggregateId,
    string LandParcelId,
    IReadOnlyCollection<string> HomeIds,
    string Name,
    string? Description,
    ProjectType ProjectType,
    string InitiatedBy) : DomainEvent(AggregateId);
