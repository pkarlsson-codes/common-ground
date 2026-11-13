namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>Detail read model for a purchase workflow.</summary>
public class PurchaseWorkflowView
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the associated ground lease ID.</summary>
    public string GroundLeaseTemplateId { get; set; } = string.Empty;

    /// <summary>Gets or sets the land parcel ID.</summary>
    public string LandParcelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the home ID.</summary>
    public string HomeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the homeowner application this workflow was initiated from, or null for manual/resale-initiated workflows.</summary>
    public string? ApplicationId { get; set; }

    /// <summary>Gets or sets the buying household, or null for manual/resale-initiated workflows.</summary>
    public string? HouseholdId { get; set; }

    /// <summary>Gets or sets the current workflow phase.</summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>Gets or sets the user who initiated the workflow.</summary>
    public string InitiatedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets the buyer's full name.</summary>
    public string BuyerFullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the buyer's email address.</summary>
    public string BuyerEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the buyer's phone number.</summary>
    public string? BuyerPhone { get; set; }

    /// <summary>Gets or sets the purchase price.</summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>Gets or sets when the workflow was created.</summary>
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

    /// <summary>Gets or sets the eligibility verification details.</summary>
    public EligibilityDetailsEntry? EligibilityDetails { get; set; }

    /// <summary>Gets or sets the appraisal details.</summary>
    public AppraisalDetailsEntry? AppraisalDetails { get; set; }

    /// <summary>Gets or sets the financing commitment details.</summary>
    public FinancingDetailsEntry? FinancingDetails { get; set; }

    /// <summary>Gets or sets the inspection result.</summary>
    public InspectionResultEntry? InspectionResult { get; set; }

    /// <summary>Gets or sets the title clearance details.</summary>
    public TitleDetailsEntry? TitleDetails { get; set; }

    /// <summary>Gets or sets the lease execution details.</summary>
    public LeaseExecutionDetailsEntry? LeaseExecutionDetails { get; set; }

    /// <summary>Gets or sets the scheduled closing date.</summary>
    public DateOnly? ScheduledClosingDate { get; set; }

    /// <summary>Gets or sets the actual closing date.</summary>
    public DateOnly? ActualClosingDate { get; set; }

    /// <summary>Gets or sets closing notes.</summary>
    public string? ClosingNotes { get; set; }

    /// <summary>Gets or sets the failure reason.</summary>
    public string? FailureReason { get; set; }

    /// <summary>Gets or sets the withdrawal reason.</summary>
    public string? WithdrawalReason { get; set; }

    /// <summary>Gets or sets the stalled reason.</summary>
    public string? StalledReason { get; set; }

    /// <summary>Gets or sets the date the workflow was stalled.</summary>
    public DateOnly? StalledDate { get; set; }

    /// <summary>Gets or sets the resolution target date.</summary>
    public DateOnly? ResolutionTargetDate { get; set; }
}

/// <summary>Eligibility verification details.</summary>
public class EligibilityDetailsEntry
{
    /// <summary>Gets or sets the date eligibility was verified.</summary>
    public DateOnly VerifiedDate { get; set; }

    /// <summary>Gets or sets optional notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets the household's annual income as the documents showed it.</summary>
    public decimal VerifiedAnnualIncome { get; set; }

    /// <summary>Gets or sets the number of people in the household as the documents showed it.</summary>
    public int VerifiedHouseholdSize { get; set; }

    /// <summary>Gets or sets the limit the income was checked against, or null when the purchase has no limit.</summary>
    public decimal? IncomeLimit { get; set; }

    /// <summary>Gets or sets the percent of the level the income is over, or null for the lowest band or no schedule.</summary>
    public int? IncomeBandOverPercentOfAmi { get; set; }

    /// <summary>Gets or sets the percent of the level the income is at or under, or null above the highest level or no schedule.</summary>
    public int? IncomeBandUpToPercentOfAmi { get; set; }
}

/// <summary>One level of the schedule a purchase carries.</summary>
public class IncomeLimitLevelEntryView
{
    /// <summary>Gets or sets the percent of area median income.</summary>
    public int PercentOfAmi { get; set; }

    /// <summary>Gets or sets the limits for 1 to 8 people.</summary>
    public List<decimal> Limits { get; set; } = [];
}

/// <summary>Appraisal details.</summary>
public class AppraisalDetailsEntry
{
    /// <summary>Gets or sets the appraised value.</summary>
    public decimal AppraisedValue { get; set; }

    /// <summary>Gets or sets the appraisal date.</summary>
    public DateOnly AppraisalDate { get; set; }

    /// <summary>Gets or sets the appraiser name.</summary>
    public string AppraiserName { get; set; } = string.Empty;

    /// <summary>Gets or sets optional notes.</summary>
    public string? Notes { get; set; }
}

/// <summary>Financing commitment details.</summary>
public class FinancingDetailsEntry
{
    /// <summary>Gets or sets the lender name.</summary>
    public string LenderName { get; set; } = string.Empty;

    /// <summary>Gets or sets the committed loan amount.</summary>
    public decimal CommittedAmount { get; set; }

    /// <summary>Gets or sets the preapproval letter date.</summary>
    public DateOnly PreapprovalLetterDate { get; set; }
}

/// <summary>Inspection result details.</summary>
public class InspectionResultEntry
{
    /// <summary>Gets or sets the date of inspection.</summary>
    public DateOnly InspectionDate { get; set; }

    /// <summary>Gets or sets the inspector name.</summary>
    public string InspectorName { get; set; } = string.Empty;

    /// <summary>Gets or sets optional notes.</summary>
    public string? Notes { get; set; }
}

/// <summary>Title clearance details.</summary>
public class TitleDetailsEntry
{
    /// <summary>Gets or sets the date the title was cleared.</summary>
    public DateOnly ClearedDate { get; set; }

    /// <summary>Gets or sets the title company name.</summary>
    public string? TitleCompanyName { get; set; }
}

/// <summary>Lease execution details.</summary>
public class LeaseExecutionDetailsEntry
{
    /// <summary>Gets or sets the date the lease was signed.</summary>
    public DateOnly SignedDate { get; set; }
}
