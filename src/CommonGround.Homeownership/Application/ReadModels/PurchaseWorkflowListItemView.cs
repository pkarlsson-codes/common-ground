namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>Summary read model for purchase workflow list views.</summary>
public class PurchaseWorkflowListItemView
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the associated ground lease ID.</summary>
    public string GroundLeaseTemplateId { get; set; } = string.Empty;

    /// <summary>Gets or sets the buyer's full name.</summary>
    public string BuyerFullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the purchase price.</summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>Gets or sets the current workflow phase.</summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>Gets or sets when the workflow was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
