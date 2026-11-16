using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.Resales.Events;

/// <summary>Raised when the trust decides to buy the home itself at the purchase option price.</summary>
/// <param name="ExercisedDate">The day the trust decided.</param>
public sealed record PurchaseOptionExercised(
    string AggregateId,
    DateOnly ExercisedDate,
    string? Notes,
    string ExercisedBy) : DomainEvent(AggregateId);
