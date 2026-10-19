namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>Summary read model for household list displays and primary-contact lookups.</summary>
public class HouseholdListItemView
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the primary member's name.</summary>
    public string PrimaryMemberName { get; set; } = string.Empty;

    /// <summary>Gets or sets the primary member's email.</summary>
    public string PrimaryMemberEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the primary member's phone.</summary>
    public string PrimaryMemberPhone { get; set; } = string.Empty;

    /// <summary>Gets or sets household size.</summary>
    public int HouseholdSize { get; set; }

    /// <summary>Gets or sets if accessibility is required.</summary>
    public bool AccessibilityRequired { get; set; }

    /// <summary>Gets or sets id of user who registered household.</summary>
    public string RegisteredBy { get; set; } = string.Empty;

    /// <summary>Gets or sets registered date/time.</summary>
    public DateTimeOffset RegisteredAt { get; set; }
}
