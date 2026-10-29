namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>Summary read model for household list displays and primary-contact lookups.</summary>
public class HouseholdListItemView
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the primary member's identifier.</summary>
    public string PrimaryMemberId { get; set; } = string.Empty;

    /// <summary>Gets or sets the primary member's name.</summary>
    public string PrimaryMemberName { get; set; } = string.Empty;

    /// <summary>Gets or sets the primary member's email.</summary>
    public string PrimaryMemberEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the primary member's phone.</summary>
    public string PrimaryMemberPhone { get; set; } = string.Empty;

    /// <summary>Gets or sets every member's name and email, for search across the whole household.</summary>
    public List<HouseholdMemberEntry> Members { get; set; } = [];

    /// <summary>Gets or sets household size.</summary>
    public int HouseholdSize { get; set; }

    /// <summary>Gets or sets if accessibility is required.</summary>
    public bool AccessibilityRequired { get; set; }

    /// <summary>Gets or sets housing counseling completion status.</summary>
    public bool IsHousingCounselingCompleted { get; set; }

    /// <summary>Gets or sets CLT orientation completion status.</summary>
    public bool IsCltOrientationCompleted { get; set; }

    /// <summary>Gets or sets the current mortgage prequalification's expiry date, if one is on record.</summary>
    public DateOnly? MortgagePrequalificationExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets the expiry date of a prequalification that has lapsed and not yet been replaced.
    /// Distinguishes a household that needs re-verification from one that was never prequalified.
    /// </summary>
    public DateOnly? MortgagePrequalificationLapsedOn { get; set; }

    /// <summary>Gets or sets id of user who registered household.</summary>
    public string RegisteredBy { get; set; } = string.Empty;

    /// <summary>Gets or sets registered date/time.</summary>
    public DateTimeOffset RegisteredAt { get; set; }

    /// <summary>
    /// Whether readiness is complete and the prequalification is still valid on <paramref name="date"/>.
    /// Mirrors the selection guard in <c>HomeownerApplication.SelectForHome</c>.
    /// </summary>
    public bool IsReadyForSelectionOn(DateOnly date) =>
        IsHousingCounselingCompleted &&
        IsCltOrientationCompleted &&
        MortgagePrequalificationExpiryDate >= date;

    /// <summary>
    /// Whether the household held a prequalification that is no longer valid on <paramref name="date"/>:
    /// either already lapsed, or past its expiry date before the scheduled lapse has run.
    /// </summary>
    public bool NeedsReverificationOn(DateOnly date) =>
        MortgagePrequalificationLapsedOn is not null ||
        MortgagePrequalificationExpiryDate < date;
}
