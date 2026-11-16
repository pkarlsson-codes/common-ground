namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>Read model for a home selection: its candidates as snapshotted at start, and how it ended.</summary>
public class HomeSelectionView
{
    public string Id { get; set; } = string.Empty;

    public string HomeId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string StartedBy { get; set; } = string.Empty;

    public DateTimeOffset StartedAt { get; set; }

    /// <summary>Gets or sets the home's income limit as a percent of AMI when the selection started, or null when it had none.</summary>
    public int? IncomeLimitPercentOfAmi { get; set; }

    /// <summary>Gets or sets the effective date of the income limit schedule in effect when the selection started, or null when none was.</summary>
    public DateOnly? IncomeLimitsEffectiveFrom { get; set; }

    /// <summary>Gets or sets the waiting list as it stood when the selection started.</summary>
    public List<HomeSelectionCandidateEntry> Candidates { get; set; } = [];

    /// <summary>Gets or sets the chosen application, once the selection is completed.</summary>
    public string? SelectedApplicationId { get; set; }

    /// <summary>Gets or sets how the household was chosen: "Priority" or "Lottery".</summary>
    public string? SelectionMethod { get; set; }

    /// <summary>Gets or sets the priorities staff cited, when chosen by priority.</summary>
    public List<string> Priorities { get; set; } = [];

    /// <summary>Gets or sets staff's explanation of the choice, when chosen by priority.</summary>
    public string? SelectionReason { get; set; }

    /// <summary>Gets or sets the lottery seed, when chosen by lottery.</summary>
    public int? LotterySeed { get; set; }

    /// <summary>Gets or sets the applications the lottery drew from.</summary>
    public List<string> LotteryPoolApplicationIds { get; set; } = [];

    /// <summary>Gets or sets the applications left out of the lottery because they could not be selected.</summary>
    public List<string> LotteryIneligibleApplicationIds { get; set; } = [];

    public string? CancellationReason { get; set; }

    /// <summary>Gets or sets why the purchase the selection led to fell through, once it has.</summary>
    public string? FellThroughReason { get; set; }

    /// <summary>Gets or sets the date the purchase the selection led to fell through, once it has.</summary>
    public DateOnly? FellThroughDate { get; set; }

    /// <summary>Gets or sets who completed or cancelled the selection.</summary>
    public string? ClosedBy { get; set; }

    /// <summary>Gets or sets when the selection was completed or cancelled.</summary>
    public DateTimeOffset? ClosedAt { get; set; }
}

/// <summary>A candidate's priority facts, frozen when the selection started.</summary>
public class HomeSelectionCandidateEntry
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

    /// <summary>Gets or sets the home's income limit for the household's size, or null when the home has none.</summary>
    public decimal? IncomeLimit { get; set; }

    /// <summary>Gets or sets a value indicating whether the household's income was over <see cref="IncomeLimit"/>, so it can't be selected.</summary>
    public bool IsOverIncomeLimit { get; set; }
}
