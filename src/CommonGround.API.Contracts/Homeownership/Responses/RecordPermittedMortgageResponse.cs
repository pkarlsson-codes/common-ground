namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>Response after recording a permitted mortgage.</summary>
/// <param name="MortgageId">The new mortgage's ID.</param>
public sealed record RecordPermittedMortgageResponse(string MortgageId);
