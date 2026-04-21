using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when the purchase is finalized and ownership transferred.</summary>
public sealed record PurchaseClosed(
    string AggregateId,
    DateOnly ActualClosingDate,
    string? ClosingNotes,
    string RecordedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
