namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>Summary read model for homeowner application list displays.</summary>
public class HomeownerApplicationListItemView
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the household id.</summary>
    public string HouseholdId { get; set; } = string.Empty;

    /// <summary>Gets or sets the household size.</summary>
    public int HouseholdSize { get; set; }

    /// <summary>Gets or sets whether accessibility accommodations are required.</summary>
    public bool AccessibilityRequired { get; set; }

    /// <summary>Gets or sets the current application phase.</summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>Gets or sets when the application was submitted.</summary>
    public DateTimeOffset SubmittedAt { get; set; }
}