using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when a permitted mortgage is marked paid off. It then gets no copies of notices.</summary>
public sealed record PermittedMortgagePaidOff(
    string AggregateId,
    string MortgageId,
    DateOnly PaidOffOn,
    string RecordedBy) : DomainEvent(AggregateId);
