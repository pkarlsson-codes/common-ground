using CommonGround.Homeownership.Domain.HeirTransfers;

namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>Read model for a transfer of a deceased homeowner's home to an heir.</summary>
public class HeirTransferView
{
    /// <summary>The statuses of a transfer that is still open: nothing is decided yet.</summary>
    public static readonly string[] OpenStatuses = [HeirTransferStatus.Open.DisplayName, HeirTransferStatus.HeirRecorded.DisplayName];

    /// <summary>Gets or sets the transfer ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the lease the home is held on.</summary>
    public string GroundLeaseId { get; set; } = string.Empty;

    /// <summary>Gets or sets the home.</summary>
    public string HomeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the lease's homeowner when the transfer opened.</summary>
    public string? DeceasedHomeownerFullName { get; set; }

    /// <summary>Gets or sets Open, Heir recorded, Completed or Closed.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the day the homeowner died.</summary>
    public DateOnly DiedOn { get; set; }

    /// <summary>Gets or sets the day the executor told the trust.</summary>
    public DateOnly NoticeReceivedOn { get; set; }

    /// <summary>Gets or sets the last day the lease gave the executor to tell the trust.</summary>
    public DateOnly NoticeDueOn { get; set; }

    /// <summary>Gets or sets whether the notice came after <see cref="NoticeDueOn"/>.</summary>
    public bool NoticeWasLate { get; set; }

    /// <summary>Gets or sets who told the trust.</summary>
    public string NotifiedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets how to reach them.</summary>
    public string? NotifiedByContact { get; set; }

    /// <summary>Gets or sets what staff noted when opening.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets the heir's full name, once recorded.</summary>
    public string? HeirFullName { get; set; }

    /// <summary>Gets or sets the heir's email address.</summary>
    public string? HeirEmail { get; set; }

    /// <summary>Gets or sets the heir's phone number.</summary>
    public string? HeirPhone { get; set; }

    /// <summary>Gets or sets Spouse, Child, HouseholdMember or Other, once an heir is recorded.</summary>
    public string? Relationship { get; set; }

    /// <summary>Gets or sets the day a household member moved in.</summary>
    public DateOnly? HeirMovedInOn { get; set; }

    /// <summary>Gets or sets the heir's verified annual income, for any other heir.</summary>
    public decimal? HeirVerifiedAnnualIncome { get; set; }

    /// <summary>Gets or sets the heir's household size, for any other heir.</summary>
    public int? HeirVerifiedHouseholdSize { get; set; }

    /// <summary>Gets or sets the income documents, as Documents file IDs.</summary>
    public List<string> IncomeDocumentIds { get; set; } = [];

    /// <summary>Gets or sets the most the heir's household could earn, when the home has a limit.</summary>
    public decimal? IncomeLimit { get; set; }

    /// <summary>Gets or sets the home's limit as a percent of area median income, when it has one.</summary>
    public int? IncomeLimitPercentOfAmi { get; set; }

    /// <summary>Gets or sets the day the schedule that set the limit took effect.</summary>
    public DateOnly? IncomeLimitsEffectiveFrom { get; set; }

    /// <summary>Gets or sets the day the home passed to the heir, once completed.</summary>
    public DateOnly? TransferredOn { get; set; }

    /// <summary>Gets or sets the signed Letter of Agreement, as a Documents file ID.</summary>
    public string? LetterOfAgreementDocumentId { get; set; }

    /// <summary>Gets or sets the signed Attorney's Acknowledgment, as a Documents file ID.</summary>
    public string? AttorneyAcknowledgementDocumentId { get; set; }

    /// <summary>Gets or sets what staff noted when completing.</summary>
    public string? CompletionNotes { get; set; }

    /// <summary>Gets or sets HeirNotQualified, HeirDeclined or Other, once closed.</summary>
    public string? ClosedReason { get; set; }

    /// <summary>Gets or sets what staff noted when closing.</summary>
    public string? ClosedNotes { get; set; }

    /// <summary>Gets or sets when the transfer was closed.</summary>
    public DateTimeOffset? ClosedAt { get; set; }

    /// <summary>Gets or sets when the transfer was opened.</summary>
    public DateTimeOffset OpenedAt { get; set; }
}
