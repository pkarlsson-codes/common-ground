using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.ImprovementRequests.Events;

/// <summary>Raised when the approved work is finished and the trust confirms the credit.</summary>
/// <param name="CompletedOn">The day the work finished.</param>
/// <param name="ActualCost">What the work cost, from the receipts.</param>
/// <param name="ReceiptDocumentIds">The receipts, as files in Documents. At least one.</param>
/// <param name="Notes">Anything staff noted, such as the inspection.</param>
public sealed record ImprovementWorkCompleted(
    string AggregateId,
    string GroundLeaseId,
    DateOnly CompletedOn,
    Money ActualCost,
    IReadOnlyList<string> ReceiptDocumentIds,
    Money ConfirmedCredit,
    string? Notes,
    string CompletedBy) : DomainEvent(AggregateId);
