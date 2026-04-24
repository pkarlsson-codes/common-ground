namespace CommonGround.Stewardship.Infrastructure.Persistence;

/// <summary>Denormalized read model for a foreclosure prevention case.</summary>
public sealed class ForeclosurePreventionCaseView
{
    /// <summary>Gets or sets the foreclosure prevention case ID.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the ground lease ID.</summary>
    public string GroundLeaseId { get; set; } = string.Empty;
    /// <summary>Gets or sets the trigger for foreclosure prevention.</summary>
    public string Trigger { get; set; } = string.Empty;
    /// <summary>Gets or sets the current status of the case.</summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>Gets or sets the outcome of the case resolution.</summary>
    public string? Outcome { get; set; }
    /// <summary>Gets or sets the date the case was opened.</summary>
    public DateOnly OpenedDate { get; set; }
    /// <summary>Gets or sets the date the case was resolved, if applicable.</summary>
    public DateOnly? ResolvedDate { get; set; }
    /// <summary>Gets or sets the list of counseling referrals.</summary>
    public List<CounselingReferralRecord> CounselingReferrals { get; set; } = [];
    /// <summary>Gets or sets the list of forbearance requests.</summary>
    public List<ForbearanceRequestRecord> ForbearanceRequests { get; set; } = [];
    /// <summary>Gets or sets the list of cure period activations.</summary>
    public List<CurePeriodActivationRecord> CurePeriodActivations { get; set; } = [];
}

/// <summary>Record of a counseling referral within a foreclosure prevention case.</summary>
public sealed class CounselingReferralRecord
{
    /// <summary>Gets or sets the counseling referral ID.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the name of the counseling agency.</summary>
    public string AgencyName { get; set; } = string.Empty;
    /// <summary>Gets or sets the date of the referral.</summary>
    public DateOnly ReferralDate { get; set; }
    /// <summary>Gets or sets the outcome of the counseling, if applicable.</summary>
    public string? Outcome { get; set; }
    /// <summary>Gets or sets the date the counseling outcome was recorded.</summary>
    public DateOnly? OutcomeDate { get; set; }
}

/// <summary>Record of a forbearance request within a foreclosure prevention case.</summary>
public sealed class ForbearanceRequestRecord
{
    /// <summary>Gets or sets the forbearance request ID.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the name of the lender.</summary>
    public string LenderName { get; set; } = string.Empty;
    /// <summary>Gets or sets the date forbearance was requested.</summary>
    public DateOnly RequestedDate { get; set; }
    /// <summary>Gets or sets the outcome of the forbearance request, if applicable.</summary>
    public string? Outcome { get; set; }
    /// <summary>Gets or sets the date the forbearance outcome was recorded.</summary>
    public DateOnly? OutcomeDate { get; set; }
    /// <summary>Gets or sets notes about the forbearance request.</summary>
    public string? Notes { get; set; }
}

/// <summary>Record of a cure period activation within a foreclosure prevention case.</summary>
public sealed class CurePeriodActivationRecord
{
    /// <summary>Gets or sets the cure period activation ID.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the LRA (Local Recipient Agency) ID.</summary>
    public string LraId { get; set; } = string.Empty;
    /// <summary>Gets or sets the date the cure period was activated.</summary>
    public DateOnly ActivationDate { get; set; }
    /// <summary>Gets or sets the number of days in the cure period.</summary>
    public int CurePeriodDays { get; set; }
    /// <summary>Gets or sets the deadline for curing the default.</summary>
    public DateOnly CureDeadline { get; set; }
}
