using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>
/// Raised when an insurance certificate recorded in error is removed, so it no longer counts toward the lease's cover.
/// </summary>
public sealed record InsuranceCertificateRemoved(
    string AggregateId,
    string CertificateId,
    string Reason,
    string RemovedBy) : DomainEvent(AggregateId);
