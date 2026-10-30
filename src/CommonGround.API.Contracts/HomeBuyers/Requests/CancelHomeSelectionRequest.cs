namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Request to cancel an open home selection without choosing anyone.</summary>
public sealed record CancelHomeSelectionRequest(string Reason);
