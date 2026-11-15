using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when a recorded appraisal is corrected. Carries the full set of values that now stand and the reason.</summary>
public sealed record AppraisalCorrected(
    string AggregateId,
    decimal AppraisedValue,
    DateOnly AppraisalDate,
    string AppraiserName,
    string? AppraisalNotes,
    string Reason,
    string CorrectedBy) : DomainEvent(AggregateId);
