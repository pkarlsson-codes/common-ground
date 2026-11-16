namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>
/// Read model for the home selections a household was passed over in, across all its applications.
/// Keyed by household ID, so the history carries over when the household applies again.
/// </summary>
public class HouseholdPassOversView
{
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the pass-overs, oldest first.</summary>
    public List<HouseholdPassOverEntry> PassOvers { get; set; } = [];
}

/// <summary>One home selection in which another household was chosen.</summary>
public class HouseholdPassOverEntry
{
    public string ApplicationId { get; set; } = string.Empty;

    public string SelectionId { get; set; } = string.Empty;

    public string HomeId { get; set; } = string.Empty;

    public DateTimeOffset PassedOverAt { get; set; }
}
