using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.Buildings.Events;

/// <summary>Raised when a new building is registered on a CLT land parcel.</summary>
public sealed record BuildingRegistered(
    string AggregateId,
    string LandParcelId,
    string BuildingType,
    int Bedrooms,
    decimal Bathrooms,
    decimal SquareFootage,
    int YearBuilt,
    decimal InitialAssessedValue,
    string RegisteredBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
