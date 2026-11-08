namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record that the trust will buy a home at the purchase option price.</summary>
/// <param name="ExercisedDate">The day the trust decided.</param>
/// <param name="Notes">Anything staff want to note about the decision.</param>
public sealed record ExercisePurchaseOptionRequest(DateOnly ExercisedDate, string? Notes);
