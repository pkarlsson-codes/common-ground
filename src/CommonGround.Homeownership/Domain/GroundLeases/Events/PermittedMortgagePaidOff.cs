using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when a permitted mortgage is marked paid off. It then gets no copies of notices.</summary>
/// <param name="AggregateId">The ground lease ID.</param>
/// <param name="MortgageId">The mortgage's ID.</param>
/// <param name="PaidOffOn">The day it was paid off.</param>
/// <param name="RecordedBy">The staff user who recorded it.</param>
public sealed record PermittedMortgagePaidOff(
    string AggregateId,
    string MortgageId,
    DateOnly PaidOffOn,
    string RecordedBy) : DomainEvent(AggregateId);
