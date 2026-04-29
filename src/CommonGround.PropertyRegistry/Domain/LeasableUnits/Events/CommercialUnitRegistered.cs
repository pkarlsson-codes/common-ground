using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits.Events;

/// <summary>Raised when a new commercial unit is registered on a CLT land parcel.</summary>
public sealed record CommercialUnitRegistered(
    string AggregateId,
    string LandParcelId,
    decimal SquareFootage,
    int YearBuilt,
    decimal InitialAssessedValue,
    string RegisteredBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
