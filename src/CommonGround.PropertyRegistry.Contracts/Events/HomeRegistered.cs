using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Contracts.Events;

/// <summary>
/// Integration event published when a new home is registered on a CLT land parcel.
/// Consumed by ApplicantSelection to resolve a home's land parcel for applicant selection.
/// </summary>
public sealed record HomeRegistered(
    string HomeId,
    string LandParcelId) : IntegrationEvent;
