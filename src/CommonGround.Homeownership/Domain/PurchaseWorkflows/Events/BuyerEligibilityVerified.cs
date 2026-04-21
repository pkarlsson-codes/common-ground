using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when buyer eligibility requirements are verified.</summary>
public sealed record BuyerEligibilityVerified(
    string AggregateId,
    DateOnly VerifiedDate,
    string? EligibilityNotes,
    string RecordedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
