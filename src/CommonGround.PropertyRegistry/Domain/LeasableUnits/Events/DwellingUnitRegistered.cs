using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits.Events;

/// <summary>Raised when a new dwelling unit is registered on a CLT land parcel.</summary>
public sealed record DwellingUnitRegistered(
    string AggregateId,
    string LandParcelId,
    string UnitType,
    int Bedrooms,
    decimal Bathrooms,
    decimal SquareFootage,
    int YearBuilt,
    decimal InitialAssessedValue,
    string RegisteredBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
