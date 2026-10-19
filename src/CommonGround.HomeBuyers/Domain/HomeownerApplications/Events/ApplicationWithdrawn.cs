using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

/// <summary>Application withdrawn event.</summary>
public sealed record ApplicationWithdrawn(
    string AggregateId,
    DateOnly WithdrawnDate,
    string? Reason,
    string RecordedBy) : DomainEvent(AggregateId);