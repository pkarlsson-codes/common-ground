namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Request to set a home's income limit.</summary>
/// <param name="PercentOfAmi">The limit as a percent of AMI, from 1 to 200, or null for no income limit.</param>
public sealed record SetHomeIncomeLimitRequest(int? PercentOfAmi);
