namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>
/// Read model for a home selection: the household chosen for a home, how, and the whole waiting list as it was
/// compared against the home at that moment.
/// </summary>
public class HomeSelectionView
{
    public string Id { get; set; } = string.Empty;

    public string HomeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the chosen application.</summary>
    public string SelectedApplicationId { get; set; } = string.Empty;

    /// <summary>Gets or sets the chosen application's household.</summary>
    public string SelectedHouseholdId { get; set; } = string.Empty;

    /// <summary>Gets or sets the date staff gave the choice.</summary>
    public DateOnly SelectedDate { get; set; }

    /// <summary>Gets or sets the staff user who chose or drew.</summary>
    public string SelectedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets when the choice was recorded.</summary>
    public DateTimeOffset RecordedAt { get; set; }

    /// <summary>Gets or sets how the household was chosen: "Priority" or "Lottery".</summary>
    public string SelectionMethod { get; set; } = string.Empty;

    /// <summary>Gets or sets the ground lease template the purchase uses.</summary>
    public string GroundLeaseTemplateId { get; set; } = string.Empty;

    /// <summary>Gets or sets the purchase price.</summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>Gets or sets the home's income limit as a percent of AMI on the date, or null when it had none.</summary>
    public int? IncomeLimitPercentOfAmi { get; set; }

    /// <summary>Gets or sets the effective date of the income limit schedule in effect on the date, or null when none was.</summary>
    public DateOnly? IncomeLimitsEffectiveFrom { get; set; }

    /// <summary>Gets or sets the waiting list as it was compared against the home, longest waiting first.</summary>
    public List<HomeSelectionApplicationEntry> Applications { get; set; } = [];

    /// <summary>Gets or sets the priorities staff cited, when chosen by priority.</summary>
    public List<string> Priorities { get; set; } = [];

    /// <summary>Gets or sets staff's explanation of the choice, when chosen by priority.</summary>
    public string? SelectionReason { get; set; }

    /// <summary>Gets or sets the applications staff found tied, when a lottery drew from only those; null otherwise.</summary>
    public List<string>? LotteryTiedApplicationIds { get; set; }

    /// <summary>Gets or sets the applications the lottery drew from.</summary>
    public List<string> LotteryPoolApplicationIds { get; set; } = [];

    /// <summary>Gets or sets the lottery seed, when chosen by lottery.</summary>
    public int? LotterySeed { get; set; }
}

/// <summary>One application on the waiting list when a household was chosen, and what the choice decided for it.</summary>
public class HomeSelectionApplicationEntry
{
    public string ApplicationId { get; set; } = string.Empty;

    public string HouseholdId { get; set; } = string.Empty;

    /// <summary>Gets or sets the household's annual income as declared on the application.</summary>
    public decimal AnnualIncome { get; set; }

    /// <summary>Gets or sets the number of people in the household as declared on the application.</summary>
    public int HouseholdSize { get; set; }

    public bool RequiresAccessibility { get; set; }

    /// <summary>Gets or sets the date the household joined the waiting list.</summary>
    public DateOnly WaitingSince { get; set; }

    /// <summary>Gets or sets the home's income limit for the household's size, or null when the home had none.</summary>
    public decimal? IncomeLimit { get; set; }

    /// <summary>Gets or sets what the choice decided: "Chosen", "PassedOver" or "CouldNotBeChosen".</summary>
    public string Outcome { get; set; } = string.Empty;

    /// <summary>Gets or sets why the household couldn't be chosen, as <c>CannotBeChosenReason</c> names; empty otherwise.</summary>
    public List<string> Reasons { get; set; } = [];
}
