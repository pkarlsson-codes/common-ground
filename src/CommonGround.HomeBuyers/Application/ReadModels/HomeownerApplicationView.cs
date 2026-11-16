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
}