using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Contracts.Events;

/// <summary>
/// Integration event published when a new home is registered on a CLT land parcel.
/// Consumed by HomeBuyers to keep the home facts that selection compares candidates against, and by
/// Homeownership to name the home on purchases. <paramref name="Label"/> is the parcel address plus any unit designation.
/// <paramref name="IncomeLimitPercentOfAmi"/> is the home's income limit as a percent of area median income, or null for none.
/// </summary>
public sealed record HomeRegistered(
    string HomeId,
    string LandParcelId,
    string Label,
    int Bedrooms,
    decimal Bathrooms,
    decimal SquareFootage,
    bool IsAccessible,
    int? IncomeLimitPercentOfAmi) : IntegrationEvent;
