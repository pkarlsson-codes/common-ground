namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>
/// A home selection the household was passed over in, with how the chosen household was picked:
/// "Priority" or "Lottery".
/// </summary>
public sealed record HouseholdPassOver(
    HouseholdPassOverEntry Entry,
    string? SelectionMethod);
