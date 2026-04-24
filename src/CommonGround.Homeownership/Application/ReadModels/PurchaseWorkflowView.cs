namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>Detail read model for a purchase workflow.</summary>
public class PurchaseWorkflowView
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the associated ground lease ID.</summary>
    public string GroundLeaseId { get; set; } = string.Empty;

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

    /// <summary>Gets or sets the eligibility verification details.</summary>
    public EligibilityDetailsEntry? EligibilityDetails { get; set; }

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
