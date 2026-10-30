namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>
/// A homeowner application together with its household's current primary contact and the home
/// selections the household was passed over in, across all its applications, oldest first.
/// </summary>
public sealed record HomeownerApplicationDetails(
    HomeownerApplicationView Application,
    HouseholdContact? PrimaryContact,
    IReadOnlyList<HouseholdPassOver> PassOvers);
