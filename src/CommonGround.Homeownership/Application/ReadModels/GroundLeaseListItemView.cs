namespace CommonGround.Homeownership.Application.ReadModels;

public class GroundLeaseListItemView
{
    public string Id { get; set; } = string.Empty;

    public string LandParcelId { get; set; } = string.Empty;

    public string HomeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the purchase workflow whose closing created the lease, if any.</summary>
    public string? PurchaseWorkflowId { get; set; }

    public DateOnly LeaseStartDate { get; set; }

    public int TermYears { get; set; }

    public string Status { get; set; } = string.Empty;

    public string FormulaKind { get; set; } = string.Empty;

    public string? HomeownerFullName { get; set; }

    public string? HomeownerEmail { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
