using System.Text.Json.Serialization;

namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to open a transfer of a deceased homeowner's home to an heir (Model Lease Art 10.3).</summary>
/// <param name="DiedOn">The day the homeowner died; not in the future.</param>
/// <param name="NoticeReceivedOn">The day the executor told the trust; not before the death, not in the future. More than 90 days after the death is recorded as late.</param>
/// <param name="NotifiedBy">Who told the trust.</param>
/// <param name="NotifiedByContact">How to reach them, or null.</param>
/// <param name="Notes">Anything staff want to note, or null.</param>
public sealed record OpenHeirTransferRequest(
    DateOnly DiedOn,
    DateOnly NoticeReceivedOn,
    string NotifiedBy,
    [property: JsonRequired] string? NotifiedByContact,
    [property: JsonRequired] string? Notes);

/// <summary>Request to record who is taking the home and how they qualify. Recording again replaces the heir.</summary>
/// <param name="FullName">The heir's full name.</param>
/// <param name="Email">The heir's email address.</param>
/// <param name="Phone">The heir's phone number, or null.</param>
/// <param name="Relationship">How the heir qualifies: Spouse, Child, HouseholdMember or Other.</param>
/// <param name="MovedInOn">The day a household member moved in; required for HouseholdMember, at least a year before the death. Ignored for the others.</param>
/// <param name="VerifiedAnnualIncome">The heir's verified annual income; required for Other. Ignored for the others.</param>
/// <param name="VerifiedHouseholdSize">The heir's household size; required for Other. Ignored for the others.</param>
/// <param name="IncomeDocumentIds">The income documents as Documents file IDs; at least one for Other. Ignored for the others.</param>
public sealed record RecordHeirRequest(
    string FullName,
    string Email,
    [property: JsonRequired] string? Phone,
    string Relationship,
    [property: JsonRequired] DateOnly? MovedInOn,
    [property: JsonRequired] decimal? VerifiedAnnualIncome,
    [property: JsonRequired] int? VerifiedHouseholdSize,
    [property: JsonRequired] IReadOnlyList<string>? IncomeDocumentIds);

/// <summary>Request to complete the transfer: the lease then names the heir as homeowner.</summary>
/// <param name="TransferredOn">The day the home passes to the heir; not before the death, not in the future.</param>
/// <param name="LetterOfAgreementDocumentId">The signed Letter of Agreement, as a Documents file ID.</param>
/// <param name="AttorneyAcknowledgementDocumentId">The signed Attorney's Acknowledgment, as a Documents file ID.</param>
/// <param name="Notes">Anything staff want to note, or null.</param>
public sealed record CompleteHeirTransferRequest(
    DateOnly TransferredOn,
    string LetterOfAgreementDocumentId,
    string AttorneyAcknowledgementDocumentId,
    [property: JsonRequired] string? Notes);

/// <summary>Request to close the transfer without the home passing to the heir.</summary>
/// <param name="Reason">Why: HeirNotQualified, HeirDeclined or Other.</param>
/// <param name="Notes">Anything staff want to note, or null.</param>
public sealed record CloseHeirTransferRequest(string Reason, [property: JsonRequired] string? Notes);
