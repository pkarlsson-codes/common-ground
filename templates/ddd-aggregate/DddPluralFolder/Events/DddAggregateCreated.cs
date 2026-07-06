using CommonGround.SharedKernel.Domain.Events;

namespace DddNamespace.DddPluralFolder.Events;

/// <summary>Raised when a new DddAggregate is created.</summary>
public sealed record DddAggregateCreated(
    string AggregateId,
    string CreatedBy) : DomainEvent(AggregateId);
