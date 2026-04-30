using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.Homes.Events;

/// <summary>Raised when a new home is registered on a CLT land parcel.</summary>
public sealed record HomeRegistered(
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
