namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>An application eligible for selection, together with its household's current primary contact.</summary>
public sealed record HomeCandidate(
    HomeownerApplicationListItemView Application,
    HouseholdContact? PrimaryContact);
