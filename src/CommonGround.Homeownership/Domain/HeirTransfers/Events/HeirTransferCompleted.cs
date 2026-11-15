using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.HeirTransfers.Events;

/// <summary>Raised when the trust consents and the signed letters are in. The lease then names the heir as homeowner.</summary>
/// <param name="AggregateId">The heir transfer ID.</param>
/// <param name="GroundLeaseId">The lease the home is held on.</param>
/// <param name="TransferredOn">The day the home passed to the heir.</param>
/// <param name="LetterOfAgreementDocumentId">The signed Letter of Agreement, as a Documents file ID.</param>
/// <param name="AttorneyAcknowledgementDocumentId">The signed Attorney's Acknowledgment, as a Documents file ID.</param>
/// <param name="Notes">Anything staff want to note.</param>
/// <param name="CompletedBy">The staff user who recorded it.</param>
public sealed record HeirTransferCompleted(
    string AggregateId,
    string GroundLeaseId,
    DateOnly TransferredOn,
    string LetterOfAgreementDocumentId,
    string AttorneyAcknowledgementDocumentId,
    string? Notes,
    string CompletedBy) : DomainEvent(AggregateId);
