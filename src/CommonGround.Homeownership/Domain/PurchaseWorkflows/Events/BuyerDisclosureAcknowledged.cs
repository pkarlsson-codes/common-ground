using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when the buyer's signed disclosure acknowledgment is recorded (Art 1).</summary>
/// <param name="AggregateId">The purchase workflow's ID.</param>
/// <param name="AcknowledgedDate">The date the buyer signed.</param>
/// <param name="DisclosureNotes">Optional notes.</param>
/// <param name="DocumentId">The signed acknowledgment in Documents.</param>
/// <param name="RecordedBy">The user recording this fact.</param>
public sealed record BuyerDisclosureAcknowledged(
    string AggregateId,
    DateOnly AcknowledgedDate,
    string? DisclosureNotes,
    string DocumentId,
    string RecordedBy) : DomainEvent(AggregateId);
