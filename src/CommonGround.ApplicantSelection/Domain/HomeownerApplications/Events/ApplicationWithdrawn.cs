using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.ApplicantSelection.Domain.HomeownerApplications.Events;

/// <summary>Application withdrawn event.</summary>
public sealed record ApplicationWithdrawn(
    string AggregateId,
    DateOnly WithdrawnDate,
    string? Reason) : DomainEvent(AggregateId);