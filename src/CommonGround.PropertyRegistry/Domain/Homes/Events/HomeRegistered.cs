using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.Homes.Events;

public sealed record HomeRegistered(
    string AggregateId,
    string LandParcelId,
    string? UnitDesignation,
    HomeType UnitType,
    int Bedrooms,
    decimal Bathrooms,
    bool IsAccessible,
    int? IncomeLimitPercentOfAmi,
    decimal SquareFootage,
    int YearBuilt,
    decimal InitialAssessedValue,
    DateOnly InitiallyAssessedAt,
    string? InitialAppraiserName,
    string RegisteredBy) : DomainEvent(AggregateId);
