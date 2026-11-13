using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Contracts.Events;

/// <summary>
/// Integration event published when an applicant is selected for a specific home.
/// Consumed by Homeownership to initiate a PurchaseWorkflow, and by PropertyRegistry to show the home under contract.
/// <paramref name="AnnualIncome"/> and <paramref name="HouseholdSize"/> are what the household declared when the
/// selection started. <paramref name="IncomeLimitPercentOfAmi"/> is the home's income limit the selection judged by,
/// null when the home had none. <paramref name="IncomeLimitsEffectiveFrom"/> and <paramref name="IncomeLimitLevels"/>
/// are the income limit schedule in effect when the selection started, every level of it; both are null when no
/// schedule was in effect.
/// </summary>
public sealed record ApplicationSelected(
    string ApplicationId,
    string HouseholdId,
    string ApplicantFullName,
    string ApplicantEmail,
    string? ApplicantPhone,
    string HomeSelectionId,
    string HomeId,
    string LandParcelId,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    DateOnly SelectedDate,
    string SelectedBy,
    decimal AnnualIncome,
    int HouseholdSize,
    int? IncomeLimitPercentOfAmi,
    DateOnly? IncomeLimitsEffectiveFrom,
    IReadOnlyList<SelectedIncomeLimitLevel>? IncomeLimitLevels) : IntegrationEvent;

/// <summary>One level of the schedule a selection judged by.</summary>
/// <param name="PercentOfAmi">The percent of area median income.</param>
/// <param name="Limits">The limit for 1 to 8 people, in that order.</param>
public sealed record SelectedIncomeLimitLevel(int PercentOfAmi, IReadOnlyList<decimal> Limits);