namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Request to replace a household's circumstances.</summary>
public sealed record UpdateHouseholdCircumstancesRequest(
    bool RequiresAccessibility,
    decimal AnnualIncome);
