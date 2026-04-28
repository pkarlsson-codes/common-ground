using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

/// <summary>Raised when a new land parcel is registered in the CLT portfolio.</summary>
public sealed record LandParcelRegistered(
    string AggregateId,
    string LegalParcelNumber,
    string Address,
    decimal AreaAcres,
    DateOnly AcquisitionDate,
    string AcquisitionMethod,
    string? CovenantTerms,
    string RegisteredBy) : DomainEvent(AggregateId);
