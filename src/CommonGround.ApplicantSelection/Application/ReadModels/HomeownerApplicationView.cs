namespace CommonGround.ApplicantSelection.Application.ReadModels;

/// <summary>Detail read model for a homeowner application.</summary>
public class HomeownerApplicationView
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the applicant full name.</summary>
    public string ApplicantFullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the applicant email address.</summary>
    public string ApplicantEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the applicant phone number.</summary>
    public string? ApplicantPhone { get; set; }

    /// <summary>Gets or sets the household size.</summary>
    public int HouseholdSize { get; set; }

    /// <summary>Gets or sets the annual household income.</summary>
    public decimal AnnualIncome { get; set; }

    /// <summary>Gets or sets whether accessibility accommodations are required.</summary>
    public bool AccessibilityRequired { get; set; }

    /// <summary>Gets or sets the current application phase.</summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>Gets or sets when the application was submitted.</summary>
    public DateTimeOffset SubmittedAt { get; set; }

    /// <summary>Gets or sets the user who submitted the application.</summary>
    public string SubmittedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets the eligibility approval date.</summary>
    public DateOnly? EligibilityApprovedDate { get; set; }

    /// <summary>Gets or sets whether housing counseling has been completed.</summary>
    public bool IsHousingCounselingCompleted { get; set; }

    /// <summary>Gets or sets the housing counseling certification number.</summary>
    public string? HousingCounselingCertificationNumber { get; set; }

    /// <summary>Gets or sets the date housing counseling was completed.</summary>
    public DateOnly? HousingCounselingCompletedDate { get; set; }

    /// <summary>Gets or sets the housing counselor organization.</summary>
    public string? HousingCounselorOrganization { get; set; }

    /// <summary>Gets or sets whether CLT orientation has been completed.</summary>
    public bool IsCltOrientationCompleted { get; set; }

    /// <summary>Gets or sets the date CLT orientation was completed.</summary>
    public DateOnly? CltOrientationCompletedDate { get; set; }

    /// <summary>Gets or sets the mortgage prequalification details.</summary>
    public MortgagePrequalificationEntry? MortgagePrequalification { get; set; }

    /// <summary>Gets or sets the denial reason, if any.</summary>
    public string? DenialReason { get; set; }

    /// <summary>Gets or sets the withdrawal reason, if any.</summary>
    public string? WithdrawalReason { get; set; }

    /// <summary>Gets or sets when the application was added to the waiting list.</summary>
    public DateOnly? AddedToWaitingListDate { get; set; }

    /// <summary>Gets or sets when the applicant was selected.</summary>
    public DateOnly? SelectedDate { get; set; }

    /// <summary>Gets or sets the selected home identifier.</summary>
    public string? SelectedHomeId { get; set; }

    /// <summary>Gets or sets the selected land parcel identifier.</summary>
    public string? SelectedLandParcelId { get; set; }

    /// <summary>Gets or sets who selected the applicant.</summary>
    public string? SelectedBy { get; set; }
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
}