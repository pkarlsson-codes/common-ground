namespace CommonGround.HomeBuyers.Application.ReadModels;

public class HouseholdView
{
    public string Id { get; set; } = string.Empty;

    public List<HouseholdMemberEntry> Members { get; set; } = [];

    public int HouseholdSize { get; set; }

    public decimal AnnualIncome { get; set; }

    public bool AccessibilityRequired { get; set; }

    public MortgagePrequalificationEntry? MortgagePrequalification { get; set; }

    public bool IsCltOrientationCompleted { get; set; }

    public DateOnly? CltOrientationCompletedDate { get; set; }

    public bool IsHousingCounselingCompleted { get; set; }

    public string? HousingCounselingCertificationNumber { get; set; }

    public DateOnly? HousingCounselingCompletedDate { get; set; }

    public string? HousingCounselorOrganization { get; set; }

    public DateTimeOffset RegisteredAt { get; set; }

    public string RegisteredBy { get; set; } = string.Empty;
}

public class HouseholdMemberEntry
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
}

public class MortgagePrequalificationEntry
{
    public string LenderName { get; set; } = string.Empty;

    public decimal ApprovedAmount { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public DateTimeOffset RecordedAt { get; set; }
}
