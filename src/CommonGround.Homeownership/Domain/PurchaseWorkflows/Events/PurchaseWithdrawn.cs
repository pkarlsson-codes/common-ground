using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when the buyer withdraws from the purchase.</summary>
public sealed record PurchaseWithdrawn(
    string AggregateId,
    DateOnly WithdrawnDate,
    string? WithdrawalReason,
    string RecordedBy) : DomainEvent(AggregateId);
