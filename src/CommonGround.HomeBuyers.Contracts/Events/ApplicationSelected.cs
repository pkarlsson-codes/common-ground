using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Contracts.Events;

/// <summary>
/// Integration event published when an applicant is selected for a specific home.
/// Consumed by Homeownership to initiate a PurchaseWorkflow, and by PropertyRegistry to show the home under contract.
/// <paramref name="AnnualIncome"/> and <paramref name="HouseholdSize"/> are what the household declared when the
/// selection started. <paramref name="IncomeLimitPercentOfAmi"/>, <paramref name="IncomeLimitsEffectiveFrom"/> and
/// <paramref name="IncomeLimits"/> are the income limit the selection judged by: the home's percent of AMI, the
/// schedule's effective date and the schedule's limits at that percent for 1 to 8 people. All three are null when
/// the home had no limit.
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
    IReadOnlyList<decimal>? IncomeLimits) : IntegrationEvent;