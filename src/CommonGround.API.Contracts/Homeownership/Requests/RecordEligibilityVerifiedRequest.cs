using System.Text.Json.Serialization;

namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record buyer eligibility verified.</summary>
/// <param name="VerifiedDate">Date eligibility was verified.</param>
/// <param name="VerifiedAnnualIncome">The household's annual income as the documents show it. Required.</param>
/// <param name="VerifiedHouseholdSize">The number of people in the household as the documents show it. Required.</param>
/// <param name="IncomeDocumentIds">The Documents the income was verified from. At least one.</param>
/// <param name="Notes">Optional notes.</param>
public sealed record RecordEligibilityVerifiedRequest(
    DateOnly VerifiedDate,
    [property: JsonRequired] decimal VerifiedAnnualIncome,
    [property: JsonRequired] int VerifiedHouseholdSize,
    [property: JsonRequired] IReadOnlyList<string> IncomeDocumentIds,
    string? Notes);
