using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

/// <summary>
/// The application was chosen for a home in a home selection. <paramref name="AnnualIncome"/> and
/// <paramref name="HouseholdSize"/> are what the household declared on the application. <paramref name="IncomeLimitPercentOfAmi"/>
/// is the home's limit the selection judged by, null when the home had none. <paramref name="IncomeLimitsEffectiveFrom"/> and
/// <paramref name="IncomeLimitLevels"/> are the schedule in effect on the selected date, null when none was.
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
    IReadOnlyList<IncomeLimitLevelEntry>? IncomeLimitLevels) : DomainEvent(AggregateId);
