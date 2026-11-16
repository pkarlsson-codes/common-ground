using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

public sealed record PropertyInspectionCompleted(
    string AggregateId,
    Guid InspectionId,
    bool OccupancyConfirmed,
    string? ConditionNotes,
    DateOnly CompletedDate,
    string RecordedBy) : DomainEvent(AggregateId);
