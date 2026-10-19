namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>A homeowner application together with its household's current primary contact.</summary>
public sealed record HomeownerApplicationDetails(
    HomeownerApplicationView Application,
    HouseholdContact? PrimaryContact);
