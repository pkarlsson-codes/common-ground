using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

public sealed record LandParcelRegistered(
    string AggregateId,
    string LegalParcelNumber,
    string Address,
    decimal AreaAcres,
    DateOnly AcquisitionDate,
    AcquisitionMethod AcquisitionMethod,
    string? CovenantTerms,
    string RegisteredBy) : DomainEvent(AggregateId);
