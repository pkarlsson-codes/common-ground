namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>A household together with every application it has submitted, newest first.</summary>
public sealed record HouseholdDetails(
    HouseholdView Household,
    IReadOnlyList<HomeownerApplicationListItemView> Applications,
    HouseholdJourney Journey);
