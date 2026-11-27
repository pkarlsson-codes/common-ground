namespace CommonGround.HomeBuyers.Application.ReadModels;

public class HomeownerApplicationView : IHomeownerApplicationPhaseView
{
    public string Id { get; set; } = string.Empty;

    public string HouseholdId { get; set; } = string.Empty;
    
    public int HouseholdSize { get; set; }

    public decimal AnnualIncome { get; set; }

    public bool AccessibilityRequired { get; set; }

    public string Phase { get; set; } = string.Empty;

    public DateTimeOffset SubmittedAt { get; set; }

    public string SubmittedBy { get; set; } = string.Empty;

    public DateOnly? ApprovedDate { get; set; }
    
    public string? DenialReason { get; set; }

    public string? WithdrawalReason { get; set; }

    public string? RemovalReason { get; set; }

    public string? RemovalNote { get; set; }

    public DateOnly? RemovedDate { get; set; }

    public DateOnly? SelectedDate { get; set; }

    public string? SelectedHomeId { get; set; }

    public string? SelectedLandParcelId { get; set; }

    public string? SelectedBy { get; set; }

    /// <summary>The day the household's income or size changed after approval, while staff haven't confirmed it still qualifies.</summary>
    public DateOnly? QualificationToConfirmSince { get; set; }

    /// <summary>While <see cref="QualificationToConfirmSince"/> is set, the income staff last checked.</summary>
    public decimal? QualifiedAnnualIncome { get; set; }

    /// <summary>While <see cref="QualificationToConfirmSince"/> is set, the household size staff last checked.</summary>
    public int? QualifiedHouseholdSize { get; set; }

    /// <summary>The day staff last confirmed the household still qualifies on changed figures, if they have.</summary>
    public DateOnly? QualificationConfirmedDate { get; set; }

    /// <summary>The staff user who last confirmed the household still qualifies, if anyone has.</summary>
    public string? QualificationConfirmedBy { get; set; }
}