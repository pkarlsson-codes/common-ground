using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <summary>Raised when a property inspection is completed.</summary>
/// <param name="AggregateId">The stewardship case identifier.</param>
/// <param name="InspectionId">The completed inspection identifier.</param>
/// <param name="OccupancyConfirmed">Whether occupancy was confirmed during the inspection.</param>
/// <param name="ConditionNotes">Optional notes on property condition.</param>
/// <param name="CompletedDate">The date the inspection was completed.</param>
/// <param name="RecordedBy">The staff member who recorded the completion.</param>
public sealed record PropertyInspectionCompleted(
    string AggregateId,
    Guid InspectionId,
    bool OccupancyConfirmed,
    string? ConditionNotes,
    DateOnly CompletedDate,
    string RecordedBy) : DomainEvent(AggregateId);
