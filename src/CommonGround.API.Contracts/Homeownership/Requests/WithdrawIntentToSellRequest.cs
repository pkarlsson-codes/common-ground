namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record that a homeowner who gave notice to sell has decided not to.</summary>
/// <param name="WithdrawnDate">The day the homeowner withdrew.</param>
/// <param name="Reason">Why the homeowner withdrew.</param>
public sealed record WithdrawIntentToSellRequest(
    DateOnly WithdrawnDate,
    string Reason);
