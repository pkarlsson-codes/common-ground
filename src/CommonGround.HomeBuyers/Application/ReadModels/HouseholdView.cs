namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>Detail read model for a household.</summary>
public class HouseholdView
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the household members.</summary>
    public List<HouseholdMemberEntry> Members { get; set; } = [];

    /// <summary>Gets or sets household size.</summary>
    public int HouseholdSize { get; set; }

    /// <summary>Gets or sets the annual household income.</summary>
    public decimal AnnualIncome { get; set; }

    /// <summary>Gets or sets if accessibility is required.</summary>
    public bool AccessibilityRequired { get; set; }

    /// <summary>Gets or sets the mortgage prequalification details.</summary>
    public MortgagePrequalificationEntry? MortgagePrequalification { get; set; }

    /// <summary>Gets or sets CLT orientation completion status.</summary>
    public bool IsCltOrientationCompleted { get; set; }

    /// <summary>Gets or sets CLT orientation completion date.</summary>
    public DateOnly? CltOrientationCompletedDate { get; set; }

    /// <summary>Gets or sets housing counseling completion status.</summary>
    public bool IsHousingCounselingCompleted { get; set; }

    /// <summary>Gets or sets certificate number for housing counseling.</summary>
    public string? HousingCounselingCertificationNumber { get; set; }

    /// <summary>Gets or sets housing counseling completion date.</summary>
    public DateOnly? HousingCounselingCompletedDate { get; set; }

    /// <summary>Gets or sets housing counseling org.</summary>
    public string? HousingCounselorOrganization { get; set; }

    /// <summary>Gets or sets registered date/time.</summary>
    public DateTimeOffset RegisteredAt { get; set; }

    /// <summary>Gets or sets id of user who registered household.</summary>
    public string RegisteredBy { get; set; } = string.Empty;
}

/// <summary>A household member.</summary>
public class HouseholdMemberEntry
{
    /// <summary>Gets or sets the member identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the member name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the member email.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the member phone.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Gets or sets whether this member is the household's primary contact.</summary>
    public bool IsPrimary { get; set; }
}

/// <summary>Mortgage prequalification details.</summary>
public class MortgagePrequalificationEntry
{
    /// <summary>Gets or sets the lender name.</summary>
    public string LenderName { get; set; } = string.Empty;

    /// <summary>Gets or sets the approved amount.</summary>
    public decimal ApprovedAmount { get; set; }

    /// <summary>Gets or sets the expiry date.</summary>
    public DateOnly ExpiryDate { get; set; }

    /// <summary>Gets or sets when the prequalification was recorded.</summary>
    public DateTimeOffset RecordedAt { get; set; }
}
