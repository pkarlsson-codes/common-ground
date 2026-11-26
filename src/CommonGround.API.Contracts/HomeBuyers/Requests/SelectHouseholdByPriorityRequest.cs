namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>
/// Request to choose a household for a home by priority from the waiting list as it is. <see cref="Priorities"/> are
/// the priority names staff cite, such as "LowerIncome" or "WaitingListTenure". The waiting list is judged on
/// <see cref="SelectedDate"/>, which can't be after today.
/// </summary>
public sealed record SelectHouseholdByPriorityRequest(
    string ApplicationId,
    IReadOnlyList<string> Priorities,
    string Reason,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    DateOnly SelectedDate);
