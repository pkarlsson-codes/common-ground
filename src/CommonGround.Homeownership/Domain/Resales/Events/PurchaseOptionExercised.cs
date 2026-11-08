using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.Resales.Events;

/// <summary>Raised when the trust decides to buy the home itself at the purchase option price.</summary>
/// <param name="AggregateId">The resale's ID.</param>
/// <param name="ExercisedDate">The day the trust decided.</param>
/// <param name="Notes">Anything staff noted about the decision.</param>
/// <param name="ExercisedBy">The user who recorded the decision.</param>
public sealed record PurchaseOptionExercised(
    string AggregateId,
    DateOnly ExercisedDate,
    string? Notes,
    string ExercisedBy) : DomainEvent(AggregateId);
