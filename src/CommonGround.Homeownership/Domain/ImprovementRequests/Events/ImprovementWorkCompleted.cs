using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.ImprovementRequests.Events;

/// <summary>Raised when the approved work is finished and the trust confirms the credit.</summary>
/// <param name="AggregateId">The improvement request ID.</param>
/// <param name="GroundLeaseId">The homeowner's lease.</param>
/// <param name="CompletedOn">The day the work finished.</param>
/// <param name="ActualCost">What the work cost, from the receipts.</param>
/// <param name="ReceiptDocumentIds">The receipts, as files in Documents. At least one.</param>
/// <param name="ConfirmedCredit">The credit that counts toward the lease's improvements.</param>
/// <param name="Notes">Anything staff noted, such as the inspection.</param>
/// <param name="CompletedBy">The staff user who recorded it.</param>
public sealed record ImprovementWorkCompleted(
    string AggregateId,
    string GroundLeaseId,
    DateOnly CompletedOn,
    Money ActualCost,
    IReadOnlyList<string> ReceiptDocumentIds,
    Money ConfirmedCredit,
    string? Notes,
    string CompletedBy) : DomainEvent(AggregateId);
