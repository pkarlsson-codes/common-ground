using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

public sealed record PurchaseClosed(
    string AggregateId,
    DateOnly ActualClosingDate,
    string? ClosingNotes,
    string RecordedBy) : DomainEvent(AggregateId);
