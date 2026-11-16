using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

public sealed record InspectionCompleted(
    string AggregateId,
    DateOnly InspectionDate,
    string InspectorName,
    string? InspectionNotes,
    string RecordedBy) : DomainEvent(AggregateId);
