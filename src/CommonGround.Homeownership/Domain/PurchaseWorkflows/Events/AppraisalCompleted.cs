using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when the home's appraisal is completed.</summary>
public sealed record AppraisalCompleted(
    string AggregateId,
    decimal AppraisedValue,
    DateOnly AppraisalDate,
    string AppraiserName,
    string? AppraisalNotes,
    string RecordedBy) : DomainEvent(AggregateId);
