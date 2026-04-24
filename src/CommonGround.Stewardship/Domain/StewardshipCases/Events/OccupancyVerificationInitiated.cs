using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <summary>Raised when an annual occupancy verification is initiated.</summary>
/// <param name="AggregateId">The stewardship case identifier.</param>
/// <param name="VerificationId">The new verification identifier.</param>
/// <param name="PeriodYear">The calendar year being verified.</param>
/// <param name="SurveyDeadline">Optional deadline for homeowner submission.</param>
/// <param name="InitiatedBy">The user who initiated the verification.</param>
/// <param name="OccurredAt">When the event occurred.</param>
public sealed record OccupancyVerificationInitiated(
    string AggregateId,
    Guid VerificationId,
    int PeriodYear,
    DateOnly? SurveyDeadline,
    string InitiatedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
