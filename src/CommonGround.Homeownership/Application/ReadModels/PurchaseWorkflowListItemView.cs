namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>Summary read model for purchase workflow list views.</summary>
public class PurchaseWorkflowListItemView
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the associated ground lease ID.</summary>
    public string GroundLeaseTemplateId { get; set; } = string.Empty;

    /// <summary>Gets or sets the home being purchased.</summary>
    public string HomeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the buying household, when the purchase was started by selecting an applicant.</summary>
    public string? HouseholdId { get; set; }

    /// <summary>Gets or sets the buyer's full name.</summary>
    public string BuyerFullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the purchase price.</summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>Gets or sets the current workflow phase.</summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>Gets or sets when the workflow was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets when the workflow last moved on, i.e. when it started waiting on its current step.</summary>
    public DateTimeOffset StepEnteredAt { get; set; }

    /// <summary>Gets or sets the scheduled closing date, once closing is scheduled.</summary>
    public DateOnly? ScheduledClosingDate { get; set; }

    /// <summary>Gets or sets the steps recorded so far, by <c>PurchaseStep</c> name.</summary>
    public List<string> RecordedSteps { get; set; } = [];

    /// <summary>Gets or sets the <c>PurchaseStep</c> the purchase waits on next.</summary>
    public string NextStep { get; set; } = string.Empty;
}
