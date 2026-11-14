namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Request to take an approved application off the waiting list.</summary>
/// <param name="Reason">Why: NoLongerEligible, NoResponse, FalseInformation or Other.</param>
/// <param name="Note">Staff's note; required when <paramref name="Reason"/> is Other.</param>
/// <param name="RemovedDate">The day of removal; not in the future or before the approved date.</param>
public sealed record RemoveFromWaitingListRequest(
    string Reason,
    string? Note,
    DateOnly RemovedDate);
