namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>An application list item together with its household's current primary contact.</summary>
public sealed record HomeownerApplicationSummary(
    HomeownerApplicationListItemView Application,
    HouseholdContact? PrimaryContact);
