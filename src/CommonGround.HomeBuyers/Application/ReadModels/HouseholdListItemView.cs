namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>Summary read model for household list displays and primary-contact lookups.</summary>
public class HouseholdListItemView
{
    public string Id { get; set; } = string.Empty;

    public string PrimaryMemberId { get; set; } = string.Empty;

    public string PrimaryMemberName { get; set; } = string.Empty;

    public string PrimaryMemberEmail { get; set; } = string.Empty;

    public string PrimaryMemberPhone { get; set; } = string.Empty;

    /// <summary>Gets or sets every member's name and email, for search across the whole household.</summary>
    public List<HouseholdMemberEntry> Members { get; set; } = [];

    public int HouseholdSize { get; set; }

    public bool AccessibilityRequired { get; set; }

    public bool IsHousingCounselingCompleted { get; set; }

    public bool IsCltOrientationCompleted { get; set; }

    /// <summary>
    /// Gets or sets the latest mortgage prequalification's expiry date, if one is on record. It is kept after the
    /// prequalification lapses; whether it is valid is always judged against a date.
    /// </summary>
    public DateOnly? MortgagePrequalificationExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets the expiry date of a prequalification that has lapsed and not yet been replaced.
    /// Distinguishes a household that needs re-verification from one that was never prequalified.
    /// </summary>
    public DateOnly? MortgagePrequalificationLapsedOn { get; set; }

    public string RegisteredBy { get; set; } = string.Empty;

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
