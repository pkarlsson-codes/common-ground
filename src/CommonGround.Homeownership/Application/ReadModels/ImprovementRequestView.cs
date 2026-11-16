namespace CommonGround.Homeownership.Application.ReadModels;

public class ImprovementRequestView
{
    public string Id { get; set; } = string.Empty;

    public string GroundLeaseId { get; set; } = string.Empty;

    public string HomeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the homeowner's full name when the request arrived.</summary>
    public string? HomeownerFullName { get; set; }

    /// <summary>Gets or sets Waiting for decision, Approved, Denied, Completed or Withdrawn.</summary>
    public string Status { get; set; } = string.Empty;

    public DateOnly ReceivedOn { get; set; }

    /// <summary>Gets or sets the day the trust's reply is due.</summary>
    public DateOnly ReplyDueOn { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal EstimatedCost { get; set; }

    /// <summary>Gets or sets the day the trust approved the work, if it has.</summary>
    public DateOnly? ApprovedOn { get; set; }

    /// <summary>Gets or sets the most the trust agreed to count, once approved.</summary>
    public decimal? PreApprovedCredit { get; set; }

    /// <summary>Gets or sets the lease's improvements cap when approved; null when the lease has none or it isn't approved.</summary>
    public decimal? LeaseImprovementsCap { get; set; }

    /// <summary>Gets or sets the confirmed credit the lease already had when this was approved.</summary>
    public decimal? CreditOnLeaseBefore { get; set; }

    public string? ApprovalNotes { get; set; }

    /// <summary>Gets or sets the day the trust refused the work, if it did.</summary>
    public DateOnly? DeniedOn { get; set; }

    public string? DenialReason { get; set; }

    /// <summary>Gets or sets the day the work finished, if it has.</summary>
    public DateOnly? CompletedOn { get; set; }

    public decimal? ActualCost { get; set; }

    /// <summary>Gets or sets the receipts, as Documents file IDs.</summary>
    public List<string> ReceiptDocumentIds { get; set; } = [];

    /// <summary>Gets or sets the credit that counts toward the lease's improvements; null until completed.</summary>
    public decimal? ConfirmedCredit { get; set; }

    public string? CompletionNotes { get; set; }

    /// <summary>Gets or sets the day the homeowner withdrew it, if they did.</summary>
    public DateOnly? WithdrawnOn { get; set; }

    public string? WithdrawnReason { get; set; }

    public DateTimeOffset RecordedAt { get; set; }
}
