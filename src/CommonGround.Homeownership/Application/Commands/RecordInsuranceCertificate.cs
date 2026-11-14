using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Insurance;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using Marten;
using JasperFx.Events;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Records a certificate of the homeowner's insurance on a lease (Art 9.4).</summary>
/// <param name="GroundLeaseId">The lease.</param>
/// <param name="CertificateId">The new certificate's ID, chosen by the caller.</param>
/// <param name="Insurer">The insurer.</param>
/// <param name="PolicyNumber">The policy number.</param>
/// <param name="Coverage">What the certificate says the policies cover, and when it was received.</param>
/// <param name="Notes">Notes, such as flood cover, if any.</param>
/// <param name="DocumentId">The certificate file in Documents, if one was added.</param>
/// <param name="RecordedBy">The staff user recording it.</param>
public sealed record RecordInsuranceCertificate(
    GroundLeaseId GroundLeaseId,
    InsuranceCertificateId CertificateId,
    string Insurer,
    string PolicyNumber,
    InsuranceCoverage Coverage,
    string? Notes,
    string? DocumentId,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordInsuranceCertificate"/>.</summary>
public static class RecordInsuranceCertificateHandler
{
    /// <summary>Fetches the lease for writing and the trust's today.</summary>
    public static async Task<(IEventStream<GroundLease> Lease, DateOnly Today)> LoadAsync(
        RecordInsuranceCertificate command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var lease = await session.Events
            .FetchForWriting<GroundLease>(command.GroundLeaseId.Value, cancellationToken)
            .ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return (lease, today);
    }

    /// <summary>Records the certificate on the lease.</summary>
    public static void Handle(RecordInsuranceCertificate command, IEventStream<GroundLease> lease, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(lease);

        lease.Decide(l => l.RecordInsuranceCertificate(
            command.CertificateId,
            command.Insurer,
            command.PolicyNumber,
            command.Coverage,
            command.Notes,
            command.DocumentId,
            today,
            command.RecordedBy));
    }
}
