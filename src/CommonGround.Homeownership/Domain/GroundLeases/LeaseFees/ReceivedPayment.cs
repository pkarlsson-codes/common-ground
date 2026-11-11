using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>A payment that counts toward a lease's balance, as the statement needs it: when it arrived and how much.</summary>
/// <param name="ReceivedDate">The day the trust received it.</param>
/// <param name="Amount">The amount received.</param>
public readonly record struct ReceivedPayment(DateOnly ReceivedDate, Money Amount);
