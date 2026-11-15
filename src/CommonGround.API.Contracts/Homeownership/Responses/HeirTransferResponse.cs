namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>A transfer of a deceased homeowner's home to an heir (Model Lease Art 10.3).</summary>
/// <param name="Id">The transfer ID.</param>
/// <param name="GroundLeaseId">The lease the home is held on.</param>
/// <param name="HomeId">The home.</param>
/// <param name="DeceasedHomeownerFullName">The lease's homeowner when the transfer opened.</param>
/// <param name="Status">Open, Heir recorded, Completed or Closed.</param>
/// <param name="DiedOn">The day the homeowner died.</param>
/// <param name="NoticeReceivedOn">The day the executor told the trust.</param>
/// <param name="NoticeDueOn">The last day the lease gave the executor to tell the trust: 90 days after the death.</param>
/// <param name="NoticeWasLate">Whether the notice came after <paramref name="NoticeDueOn"/>.</param>
/// <param name="NotifiedBy">Who told the trust.</param>
/// <param name="NotifiedByContact">How to reach them, or null.</param>
/// <param name="Notes">What staff noted when opening, or null.</param>
/// <param name="HeirFullName">The heir's full name, or null before an heir is recorded.</param>
/// <param name="HeirEmail">The heir's email address, or null before an heir is recorded.</param>
/// <param name="HeirPhone">The heir's phone number, or null.</param>
/// <param name="Relationship">Spouse, Child, HouseholdMember or Other, or null before an heir is recorded.</param>
/// <param name="HeirMovedInOn">The day a household member moved in, or null.</param>
/// <param name="HeirVerifiedAnnualIncome">The heir's verified annual income, for any other heir, or null.</param>
/// <param name="HeirVerifiedHouseholdSize">The heir's household size, for any other heir, or null.</param>
/// <param name="IncomeDocumentIds">The income documents as Documents file IDs.</param>
/// <param name="IncomeLimit">The most the heir's household could earn, or null when the home has no limit or the heir was not checked.</param>
/// <param name="IncomeLimitPercentOfAmi">The home's limit as a percent of area median income, or null.</param>
/// <param name="IncomeLimitsEffectiveFrom">The day the schedule that set the limit took effect, or null.</param>
/// <param name="TransferredOn">The day the home passed to the heir, or null until completed.</param>
/// <param name="LetterOfAgreementDocumentId">The signed Letter of Agreement, or null until completed.</param>
/// <param name="AttorneyAcknowledgementDocumentId">The signed Attorney's Acknowledgment, or null until completed.</param>
/// <param name="CompletionNotes">What staff noted when completing, or null.</param>
/// <param name="ClosedReason">HeirNotQualified, HeirDeclined or Other once closed, otherwise null.</param>
/// <param name="ClosedNotes">What staff noted when closing, or null.</param>
/// <param name="ClosedAt">When the transfer was closed, or null.</param>
/// <param name="OpenedAt">When the transfer was opened.</param>
public sealed record HeirTransferResponse(
    string Id,
    string GroundLeaseId,
    string HomeId,
    string? DeceasedHomeownerFullName,
    string Status,
    DateOnly DiedOn,
    DateOnly NoticeReceivedOn,
    DateOnly NoticeDueOn,
    bool NoticeWasLate,
    string NotifiedBy,
    string? NotifiedByContact,
    string? Notes,
    string? HeirFullName,
    string? HeirEmail,
    string? HeirPhone,
    string? Relationship,
    DateOnly? HeirMovedInOn,
    decimal? HeirVerifiedAnnualIncome,
    int? HeirVerifiedHouseholdSize,
    IReadOnlyList<string> IncomeDocumentIds,
    decimal? IncomeLimit,
    int? IncomeLimitPercentOfAmi,
    DateOnly? IncomeLimitsEffectiveFrom,
    DateOnly? TransferredOn,
    string? LetterOfAgreementDocumentId,
    string? AttorneyAcknowledgementDocumentId,
    string? CompletionNotes,
    string? ClosedReason,
    string? ClosedNotes,
    DateTimeOffset? ClosedAt,
    DateTimeOffset OpenedAt);

/// <summary>The ID of a transfer just opened.</summary>
/// <param name="HeirTransferId">The new transfer's ID.</param>
public sealed record OpenHeirTransferResponse(string HeirTransferId);

/// <summary>The income limits an heir who is not family is judged by.</summary>
/// <param name="PercentOfAmi">The home's limit as a percent of area median income, or null when the home has none.</param>
/// <param name="EffectiveFrom">The day the schedule in effect on the notice date took effect, or null when the home has no limit.</param>
/// <param name="Limits">The limit for 1 to 8 people, in that order; empty when the home has no limit.</param>
public sealed record HeirIncomeLimitResponse(int? PercentOfAmi, DateOnly? EffectiveFrom, IReadOnlyList<decimal> Limits);
