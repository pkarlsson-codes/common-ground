using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

public sealed record PurchaseWithdrawn(
    string AggregateId,
    DateOnly WithdrawnDate,
    string? WithdrawalReason,
    string RecordedBy) : DomainEvent(AggregateId);
