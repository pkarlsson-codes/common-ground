namespace CommonGround.Homeownership.Application.ReadModels;

public class PurchaseWorkflowListItemView : IPurchaseWorkflowPhaseView
{
    public string Id { get; set; } = string.Empty;

    public string GroundLeaseTemplateId { get; set; } = string.Empty;

    public string HomeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the buying household, when the purchase was started by selecting an applicant.</summary>
    public string? HouseholdId { get; set; }

    public string BuyerFullName { get; set; } = string.Empty;

    public decimal PurchasePrice { get; set; }

    public string Phase { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets when the workflow last moved on, i.e. when it started waiting on its current step.</summary>
    public DateTimeOffset StepEnteredAt { get; set; }

    /// <summary>Gets or sets the scheduled closing date, once closing is scheduled.</summary>
    public DateOnly? ScheduledClosingDate { get; set; }

    /// <summary>Gets or sets the steps recorded so far, by <c>PurchaseStep</c> name.</summary>
    public List<string> RecordedSteps { get; set; } = [];

    public string NextStep { get; set; } = string.Empty;
}
