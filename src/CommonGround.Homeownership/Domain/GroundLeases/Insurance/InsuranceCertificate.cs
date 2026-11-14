using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.Insurance;

/// <summary>
/// A certificate of the homeowner's insurance the trust received (Model Lease Art 9.4). A certificate recorded in
/// error is removed, with a reason, and stays on the lease for the record.
/// </summary>
public sealed class InsuranceCertificate : Entity<InsuranceCertificateId>
{
    private InsuranceCertificate(
        InsuranceCertificateId id,
        string insurer,
        string policyNumber,
        InsuranceCoverage coverage,
        string? notes,
        string? documentId)
    {
        Id = id;
        Insurer = insurer;
        PolicyNumber = policyNumber;
        Coverage = coverage;
        Notes = notes;
        DocumentId = documentId;
    }

    /// <summary>Creates a certificate from its recorded facts. The lease checks the rules before deciding on it.</summary>
    /// <param name="id">The certificate's ID.</param>
    /// <param name="insurer">The insurer.</param>
    /// <param name="policyNumber">The policy number.</param>
    /// <param name="coverage">What the certificate says the policies cover, and when it was received.</param>
    /// <param name="notes">Notes, such as flood cover, if any.</param>
    /// <param name="documentId">The certificate file in Documents, if one was added.</param>
    public static InsuranceCertificate Create(
        InsuranceCertificateId id,
        string insurer,
        string policyNumber,
        InsuranceCoverage coverage,
        string? notes,
        string? documentId)
    {
        ArgumentNullException.ThrowIfNull(coverage);

        return new InsuranceCertificate(id, insurer, policyNumber, coverage, notes, documentId);
    }

    /// <summary>Gets the insurer.</summary>
    public string Insurer { get; }

    /// <summary>Gets the policy number.</summary>
    public string PolicyNumber { get; }

    /// <summary>Gets what the certificate says the policies cover, and when it was received.</summary>
    public InsuranceCoverage Coverage { get; }

    /// <summary>Gets the notes, or null for none.</summary>
    public string? Notes { get; }

    /// <summary>Gets the certificate file in Documents, or null when none was added.</summary>
    public string? DocumentId { get; }

    /// <summary>Gets why it was removed, or null while it counts.</summary>
    public string? RemovedReason { get; private set; }

    /// <summary>Gets whether it was removed.</summary>
    public bool IsRemoved => RemovedReason is not null;

    /// <summary>Marks it removed. Called only from the lease's Apply.</summary>
    internal void MarkRemoved(string reason) => RemovedReason = reason;
}
