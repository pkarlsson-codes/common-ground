namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record that a payoff statement was printed for a resale's closing agent.</summary>
/// <param name="GoodThrough">The day the figures were worked out for, from today to 60 days ahead.</param>
public sealed record RecordPayoffStatementRequest(DateOnly GoodThrough);
