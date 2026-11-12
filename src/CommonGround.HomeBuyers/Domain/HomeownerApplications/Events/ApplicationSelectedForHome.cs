using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

/// <summary>
/// Applicant selected for home event. <paramref name="AnnualIncome"/> and <paramref name="HouseholdSize"/> are what
/// the household declared when the selection started. <paramref name="IncomeLimitPercentOfAmi"/>,
/// <paramref name="IncomeLimitsEffectiveFrom"/> and <paramref name="IncomeLimits"/> (1 to 8 people) are the limit
/// the selection judged by, all null when the home had none.
/// </summary>
public sealed record ApplicationSelectedForHome(
    string AggregateId,
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
    Money AnnualIncome,
    int HouseholdSize,
    int? IncomeLimitPercentOfAmi,
    DateOnly? IncomeLimitsEffectiveFrom,
    IReadOnlyList<Money>? IncomeLimits) : DomainEvent(AggregateId);
