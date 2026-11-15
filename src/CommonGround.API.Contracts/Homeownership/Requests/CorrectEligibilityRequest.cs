using System.Text.Json.Serialization;

namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to correct a recorded eligibility verification. Every field is the value that should stand.</summary>
/// <param name="VerifiedDate">Date eligibility was verified.</param>
/// <param name="VerifiedAnnualIncome">The household's annual income as the documents show it. Required.</param>
/// <param name="VerifiedHouseholdSize">The number of people in the household as the documents show it. Required.</param>
/// <param name="IncomeDocumentIds">Documents that replace the recorded ones. Null or empty keeps the recorded documents.</param>
/// <param name="Notes">Optional notes. Replaces the recorded notes.</param>
/// <param name="Reason">Why the record is corrected. Required.</param>
public sealed record CorrectEligibilityRequest(
    DateOnly VerifiedDate,
    [property: JsonRequired] decimal VerifiedAnnualIncome,
    [property: JsonRequired] int VerifiedHouseholdSize,
    IReadOnlyList<string>? IncomeDocumentIds,
    string? Notes,
    string? Reason);
