using System.Text.Json.Serialization;

namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Request to set a home's income limit.</summary>
/// <param name="PercentOfAmi">
/// The limit as a percent of AMI, from 1 to 200, or null for no income limit. Required, so an empty body is a
/// 400 instead of clearing the limit.
/// </param>
public sealed record SetHomeIncomeLimitRequest([property: JsonRequired] int? PercentOfAmi);
