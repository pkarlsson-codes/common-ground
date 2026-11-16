using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <param name="SurveyDeadline">Optional deadline for homeowner submission.</param>
public sealed record OccupancyVerificationInitiated(
    string AggregateId,
    Guid VerificationId,
    int PeriodYear,
    DateOnly? SurveyDeadline,
    string InitiatedBy) : DomainEvent(AggregateId);
