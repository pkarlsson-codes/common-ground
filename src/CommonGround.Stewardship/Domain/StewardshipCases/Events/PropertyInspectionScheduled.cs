using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <summary>Raised when a property inspection is scheduled.</summary>
/// <param name="AggregateId">The stewardship case identifier.</param>
/// <param name="InspectionId">The new inspection identifier.</param>
/// <param name="ScheduledDate">The scheduled date for the inspection.</param>
/// <param name="InspectorName">The name of the inspector.</param>
/// <param name="ScheduledBy">The staff member who scheduled the inspection.</param>
/// <param name="OccurredAt">When the event occurred.</param>
public sealed record PropertyInspectionScheduled(
    string AggregateId,
    Guid InspectionId,
    DateOnly ScheduledDate,
    string InspectorName,
    string ScheduledBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
