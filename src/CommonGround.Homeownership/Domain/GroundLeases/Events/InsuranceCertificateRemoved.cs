using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>
/// Raised when an insurance certificate recorded in error is removed, so it no longer counts toward the lease's cover.
/// </summary>
/// <param name="AggregateId">The ground lease.</param>
/// <param name="CertificateId">The removed certificate.</param>
/// <param name="Reason">Why it was removed.</param>
/// <param name="RemovedBy">The staff user who removed it.</param>
public sealed record InsuranceCertificateRemoved(
    string AggregateId,
    string CertificateId,
    string Reason,
    string RemovedBy) : DomainEvent(AggregateId);
