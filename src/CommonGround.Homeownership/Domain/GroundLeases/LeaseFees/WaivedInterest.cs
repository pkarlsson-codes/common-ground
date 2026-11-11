namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>Interest the trust waived on a lease, as <see cref="LeaseFeeStatement.Calculate"/> needs it.</summary>
/// <param name="WaivedOn">The day it was waived.</param>
/// <param name="Amount">How much was waived.</param>
public readonly record struct WaivedInterest(DateOnly WaivedOn, decimal Amount);
