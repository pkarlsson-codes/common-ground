using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when the buyer's signed disclosure acknowledgment is recorded (Art 1).</summary>
public sealed record BuyerDisclosureAcknowledged(
    string AggregateId,
    DateOnly AcknowledgedDate,
    string? DisclosureNotes,
    string DocumentId,
    string RecordedBy) : DomainEvent(AggregateId);
