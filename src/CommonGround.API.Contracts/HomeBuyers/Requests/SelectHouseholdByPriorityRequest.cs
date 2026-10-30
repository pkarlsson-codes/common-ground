namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>
/// Request to choose a candidate in a home selection by priority. <see cref="Priorities"/> are
/// the priority names staff cite, such as "LowerIncome" or "WaitingListTenure".
/// </summary>
public sealed record SelectHouseholdByPriorityRequest(
    string ApplicationId,
    IReadOnlyList<string> Priorities,
    string Reason,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    DateOnly SelectedDate);
