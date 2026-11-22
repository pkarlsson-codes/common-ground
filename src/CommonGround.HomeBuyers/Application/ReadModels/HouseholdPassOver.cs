namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>
/// A home selection the household was passed over in. <paramref name="SelectionMethod"/> is how the
/// chosen household was picked ("Priority" or "Lottery"); <paramref name="SelectionReason"/> is staff's
/// explanation of a priority choice, null for a lottery. <paramref name="HomeLabel"/> names the home.
/// </summary>
public sealed record HouseholdPassOver(
    HouseholdPassOverEntry Entry,
    string? HomeLabel,
    string? SelectionMethod,
    string? SelectionReason);
