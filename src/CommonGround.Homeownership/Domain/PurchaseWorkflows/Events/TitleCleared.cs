using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

public sealed record TitleCleared(
    string AggregateId,
    DateOnly ClearedDate,
    string? TitleCompanyName,
    string RecordedBy) : DomainEvent(AggregateId);
