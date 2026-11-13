using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.TrackDetails;

/// <summary>Details recorded when buyer eligibility is verified.</summary>
/// <param name="VerifiedDate">Date eligibility was verified.</param>
/// <param name="Notes">Optional notes about the verification.</param>
/// <param name="VerifiedAnnualIncome">The household's annual income as the documents show it.</param>
/// <param name="VerifiedHouseholdSize">The number of people in the household as the documents show it.</param>
/// <param name="IncomeLimit">The limit for that size the income was checked against, or null when the purchase has no limit.</param>
/// <param name="IncomeBand">The band the income falls in, or null when the purchase carries no schedule.</param>
public sealed record EligibilityDetails(
    DateOnly VerifiedDate,
    string? Notes,
    Money VerifiedAnnualIncome,
    int VerifiedHouseholdSize,
    Money? IncomeLimit,
    IncomeBand? IncomeBand);
