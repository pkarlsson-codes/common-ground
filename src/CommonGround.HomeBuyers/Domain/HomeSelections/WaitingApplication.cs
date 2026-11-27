using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.HomeSelections;

/// <summary>
/// An application on the waiting list, with what its household has on record, as a home selection compares it.
/// </summary>
/// <param name="ApplicationId">The application.</param>
/// <param name="HouseholdId">The household that applied.</param>
/// <param name="AnnualIncome">The household's annual income on the application, which follows the household.</param>
/// <param name="HouseholdSize">The number of people in the household on the application, which follows the household.</param>
/// <param name="RequiresAccessibility">Whether the household needs an accessible home.</param>
/// <param name="WaitingSince">The date the household joined the waiting list: its application's approval date.</param>
/// <param name="HasPrimaryMember">Whether the household has a primary member to buy the home.</param>
/// <param name="IsHousingCounselingCompleted">Whether the household completed housing counseling.</param>
/// <param name="IsCltOrientationCompleted">Whether the household completed CLT orientation.</param>
/// <param name="PrequalificationExpiryDate">The expiry date of the household's mortgage prequalification, if it has one.</param>
/// <param name="QualificationToConfirmSince">
/// The day the household's income or size changed after approval, while staff haven't confirmed it still qualifies.
/// </param>
public sealed record WaitingApplication(
    HomeownerApplicationId ApplicationId,
    HouseholdId HouseholdId,
    Money AnnualIncome,
    int HouseholdSize,
    bool RequiresAccessibility,
    DateOnly WaitingSince,
    bool HasPrimaryMember,
    bool IsHousingCounselingCompleted,
    bool IsCltOrientationCompleted,
    DateOnly? PrequalificationExpiryDate,
    DateOnly? QualificationToConfirmSince = null);
