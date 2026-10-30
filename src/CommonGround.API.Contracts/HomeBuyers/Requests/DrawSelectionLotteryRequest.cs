namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>
/// Request to choose a candidate in a home selection by lottery. The draw is among
/// <see cref="TiedApplicationIds"/> when given, otherwise among every candidate.
/// </summary>
public sealed record DrawSelectionLotteryRequest(
    IReadOnlyList<string>? TiedApplicationIds,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    DateOnly SelectedDate);
