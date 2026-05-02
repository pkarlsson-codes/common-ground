using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when a home inspection is completed.</summary>
public sealed record InspectionCompleted(
    string AggregateId,
    DateOnly InspectionDate,
    string InspectorName,
    string? InspectionNotes,
    string RecordedBy) : DomainEvent(AggregateId);
