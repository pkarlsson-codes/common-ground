using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.Homeownership.Domain.GroundLeases.Insurance;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Removes an insurance certificate recorded in error.</summary>
/// <param name="GroundLeaseId">The lease the certificate is on.</param>
/// <param name="CertificateId">The certificate to remove.</param>
/// <param name="Reason">Why it is removed.</param>
/// <param name="RemovedBy">The staff user removing it.</param>
public sealed record RemoveInsuranceCertificate(
    GroundLeaseId GroundLeaseId,
    InsuranceCertificateId CertificateId,
    string Reason,
    UserId RemovedBy);

/// <summary>Handles <see cref="RemoveInsuranceCertificate"/>.</summary>
public static class RemoveInsuranceCertificateHandler
{
    /// <summary>Removes the certificate from the lease's cover.</summary>
    public static InsuranceCertificateRemoved Handle(RemoveInsuranceCertificate command, [WriteAggregate] GroundLease lease)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(lease);

        return lease.RemoveInsuranceCertificate(command.CertificateId, command.Reason, command.RemovedBy);
    }
}
