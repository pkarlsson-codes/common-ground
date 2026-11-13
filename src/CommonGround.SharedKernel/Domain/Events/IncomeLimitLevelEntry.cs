namespace CommonGround.SharedKernel.Domain.Events;

/// <summary>One level of an <see cref="IncomeLimitTable"/> as an event stores it.</summary>
/// <param name="PercentOfAmi">The percent of area median income.</param>
/// <param name="Limits">The limit for 1 to 8 people, in that order.</param>
public sealed record IncomeLimitLevelEntry(int PercentOfAmi, IReadOnlyList<Money> Limits);
