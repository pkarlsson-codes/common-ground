namespace CommonGround.Homeownership.Application.ReadModels;

public class PurchaseWorkflowView : IPurchaseWorkflowPhaseView
{
    public string Id { get; set; } = string.Empty;

    public string GroundLeaseTemplateId { get; set; } = string.Empty;

    public string LandParcelId { get; set; } = string.Empty;

    public string HomeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the homeowner application this workflow was initiated from, or null for manual/resale-initiated workflows.</summary>
    public string? ApplicationId { get; set; }

    /// <summary>Gets or sets the buying household, or null for manual/resale-initiated workflows.</summary>
    public string? HouseholdId { get; set; }

    public string Phase { get; set; } = string.Empty;

    public string InitiatedBy { get; set; } = string.Empty;

    public string BuyerFullName { get; set; } = string.Empty;

    public string BuyerEmail { get; set; } = string.Empty;

    public string? BuyerPhone { get; set; }

    public decimal PurchasePrice { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the annual income the household declared when selected, or null when the purchase didn't start from a selection.</summary>
    public decimal? DeclaredAnnualIncome { get; set; }

    /// <summary>Gets or sets the household size declared when selected, or null when the purchase didn't start from a selection.</summary>
    public int? DeclaredHouseholdSize { get; set; }

    /// <summary>Gets or sets the home's limit as a percent of AMI the selection judged by, or null when there is none.</summary>
    public int? IncomeLimitPercentOfAmi { get; set; }

    /// <summary>Gets or sets the effective date of the schedule in effect when the selection started, or null when none was.</summary>
    public DateOnly? IncomeLimitsEffectiveFrom { get; set; }

    /// <summary>Gets or sets that schedule's levels, lowest percent first.</summary>
    public List<IncomeLimitLevelEntryView>? IncomeLimitLevels { get; set; }

    public EligibilityDetailsEntry? EligibilityDetails { get; set; }

    public AppraisalDetailsEntry? AppraisalDetails { get; set; }

    public FinancingDetailsEntry? FinancingDetails { get; set; }

    public InspectionResultEntry? InspectionResult { get; set; }

    public TitleDetailsEntry? TitleDetails { get; set; }

    public LeaseExecutionDetailsEntry? LeaseExecutionDetails { get; set; }

    /// <summary>Gets or sets the buyer's signed disclosure acknowledgment, or null when it isn't recorded.</summary>
    public DisclosureDetailsEntry? DisclosureDetails { get; set; }

    /// <summary>Gets or sets the corrections made to recorded tracks, oldest first.</summary>
    public List<PurchaseCorrectionEntry> Corrections { get; set; } = [];

    public DateOnly? ScheduledClosingDate { get; set; }

    public DateOnly? ActualClosingDate { get; set; }

    public string? ClosingNotes { get; set; }

    public string? FailureReason { get; set; }

    public string? WithdrawalReason { get; set; }

    public string? StalledReason { get; set; }

    public DateOnly? StalledDate { get; set; }

    public DateOnly? ResolutionTargetDate { get; set; }
}

public class EligibilityDetailsEntry
{
    public DateOnly VerifiedDate { get; set; }

    public string? Notes { get; set; }

    public decimal VerifiedAnnualIncome { get; set; }

    /// <summary>Gets or sets the number of people in the household as the documents showed it.</summary>
    public int VerifiedHouseholdSize { get; set; }

    /// <summary>Gets or sets the limit the income was checked against, or null when the purchase has no limit.</summary>
    public decimal? IncomeLimit { get; set; }

    /// <summary>Gets or sets the percent of the level the income is over, or null for the lowest band or no schedule.</summary>
    public int? IncomeBandOverPercentOfAmi { get; set; }

    /// <summary>Gets or sets the percent of the level the income is at or under, or null above the highest level or no schedule.</summary>
    public int? IncomeBandUpToPercentOfAmi { get; set; }

    /// <summary>Gets or sets the Documents the income was verified from.</summary>
    public List<string> IncomeDocumentIds { get; set; } = [];
}

public class IncomeLimitLevelEntryView
{
    /// <summary>Gets or sets the percent of area median income.</summary>
    public int PercentOfAmi { get; set; }

    public List<decimal> Limits { get; set; } = [];
}

public class AppraisalDetailsEntry
{
    public decimal AppraisedValue { get; set; }

    public DateOnly AppraisalDate { get; set; }

    public string AppraiserName { get; set; } = string.Empty;

    public string? Notes { get; set; }
}

public class FinancingDetailsEntry
{
    public string LenderName { get; set; } = string.Empty;

    public decimal CommittedAmount { get; set; }

    public DateOnly PreapprovalLetterDate { get; set; }
}

public class InspectionResultEntry
{
    public DateOnly InspectionDate { get; set; }

    public string InspectorName { get; set; } = string.Empty;

    public string? Notes { get; set; }
}

public class TitleDetailsEntry
{
    public DateOnly ClearedDate { get; set; }

    public string? TitleCompanyName { get; set; }
}

public class LeaseExecutionDetailsEntry
{
    public DateOnly SignedDate { get; set; }
}

public class DisclosureDetailsEntry
{
    public DateOnly AcknowledgedDate { get; set; }

    public string? Notes { get; set; }

    public string DocumentId { get; set; } = string.Empty;
}

public class PurchaseCorrectionEntry
{
    /// <summary>Gets or sets the track corrected: Eligibility, Appraisal or Financing.</summary>
    public string Track { get; set; } = string.Empty;

    /// <summary>Gets or sets when the correction was made.</summary>
    public DateTimeOffset CorrectedAt { get; set; }

    public string CorrectedBy { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    /// <summary>Gets or sets the fields that changed, at least one.</summary>
    public List<PurchaseCorrectionChange> Changes { get; set; } = [];
}

public class PurchaseCorrectionChange
{
    /// <summary>Gets or sets the field name, such as AppraisedValue.</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>Gets or sets how to show the values: Money, Date, Number, Text or Documents.</summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>Gets or sets the value before, in invariant text, or null when there was none.</summary>
    public string? Was { get; set; }

    /// <summary>Gets or sets the value after, in invariant text, or null when there is none.</summary>
    public string? Now { get; set; }
}
