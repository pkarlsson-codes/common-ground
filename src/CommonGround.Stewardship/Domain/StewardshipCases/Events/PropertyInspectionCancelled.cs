using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <summary>Raised when a scheduled property inspection is cancelled.</summary>
/// <param name="AggregateId">The stewardship case identifier.</param>
/// <param name="InspectionId">The cancelled inspection identifier.</param>
/// <param name="Reason">The reason for cancellation.</param>
/// <param name="CancelledBy">The staff member who cancelled the inspection.</param>
/// <param name="OccurredAt">When the event occurred.</param>
public sealed record PropertyInspectionCancelled(
    string AggregateId,
    Guid InspectionId,
    string Reason,
    string CancelledBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
