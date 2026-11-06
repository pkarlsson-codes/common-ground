namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>Response after recording an intent to sell.</summary>
/// <param name="ResaleId">The resale the notice opened.</param>
public sealed record RecordIntentToSellResponse(string ResaleId);
