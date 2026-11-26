namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>
/// Request to choose a household for a home by lottery from the waiting list as it is. The draw is among
/// <see cref="TiedApplicationIds"/> when given, otherwise among the whole waiting list, leaving out households that
/// can't be chosen on <see cref="SelectedDate"/>, which can't be after today.
/// </summary>
public sealed record DrawSelectionLotteryRequest(
    IReadOnlyList<string>? TiedApplicationIds,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    DateOnly SelectedDate);
