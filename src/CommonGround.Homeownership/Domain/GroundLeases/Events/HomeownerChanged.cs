using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when the homeowner on a ground lease is changed.</summary>
public sealed record HomeownerChanged(
    string AggregateId,
    string PreviousFullName,
    string PreviousEmail,
    string? PreviousPhone,
    string NewFullName,
    string NewEmail,
    string? NewPhone,
    string ChangedBy) : DomainEvent(AggregateId);
