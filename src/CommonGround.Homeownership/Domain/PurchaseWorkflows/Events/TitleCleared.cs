using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when the title search is completed without defects.</summary>
public sealed record TitleCleared(
    string AggregateId,
    DateOnly ClearedDate,
    string? TitleCompanyName,
    string RecordedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
