using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.HeirTransfers.Events;

/// <summary>Raised when the trust consents and the signed letters are in. The lease then names the heir as homeowner.</summary>
/// <param name="TransferredOn">The day the home passed to the heir.</param>
/// <param name="LetterOfAgreementDocumentId">The signed Letter of Agreement, as a Documents file ID.</param>
/// <param name="AttorneyAcknowledgementDocumentId">The signed Attorney's Acknowledgment, as a Documents file ID.</param>
public sealed record HeirTransferCompleted(
    string AggregateId,
    string GroundLeaseId,
    DateOnly TransferredOn,
    string LetterOfAgreementDocumentId,
    string AttorneyAcknowledgementDocumentId,
    string? Notes,
    string CompletedBy) : DomainEvent(AggregateId);
