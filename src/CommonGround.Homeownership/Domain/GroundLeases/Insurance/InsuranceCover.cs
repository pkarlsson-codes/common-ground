using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.Insurance;

/// <summary>
/// Where a lease's insurance stands from the certificates that count (not removed): the date it is covered through,
/// the current certificate's coverage and its check, and when the next certificate is due.
/// </summary>
public sealed class InsuranceCover : ValueObject
{
    /// <summary>How many days before the due date a certificate counts as due.</summary>
    public const int DueWindowDays = 30;

    private InsuranceCover(DateOnly leaseStartDate, InsuranceCoverage? current, InsuranceCheck? check)
    {
        LeaseStartDate = leaseStartDate;
        Current = current;
        Check = check;
    }

    /// <summary>
    /// Works out the cover. The current certificate is the one with the latest policy end, the latest received on a
    /// tie, and the latest recorded when both match (an amended certificate received the same day). It is checked
    /// against the requirements in effect on its policy start.
    /// </summary>
    /// <param name="leaseStartDate">The lease start, when the first certificate is due.</param>
    /// <param name="certificates">The coverage of every certificate that counts, in the order recorded.</param>
    /// <param name="requirements">The insurance the lease has required.</param>
    public static InsuranceCover Of(
        DateOnly leaseStartDate,
        IEnumerable<InsuranceCoverage> certificates,
        InsuranceRequirementsHistory requirements)
    {
        ArgumentNullException.ThrowIfNull(certificates);
        ArgumentNullException.ThrowIfNull(requirements);

        // Reversed first so the stable sort puts the latest recorded first on a full tie.
        var current = certificates
            .Reverse()
            .OrderByDescending(c => c.PolicyEnd)
            .ThenByDescending(c => c.ReceivedOn)
            .FirstOrDefault();
        var check = current is null ? null : InsuranceCheck.Of(current, requirements.InEffectOn(current.PolicyStart));

        return new InsuranceCover(leaseStartDate, current, check);
    }

    /// <summary>Gets the lease start.</summary>
    public DateOnly LeaseStartDate { get; }

    /// <summary>Gets the current certificate's coverage, or null when none is on file.</summary>
    public InsuranceCoverage? Current { get; }

    /// <summary>Gets the current certificate's check, or null when none is on file.</summary>
    public InsuranceCheck? Check { get; }

    /// <summary>Gets the latest policy end among the certificates, or null when none is on file.</summary>
    public DateOnly? CoveredThrough => Current?.PolicyEnd;

    /// <summary>Gets when the next certificate is due: the covered-through date, or the lease start with none on file.</summary>
    public DateOnly DueOn => CoveredThrough ?? LeaseStartDate;

    /// <summary>Gets whether a certificate is due on a day: from <see cref="DueWindowDays"/> before <see cref="DueOn"/>.</summary>
    /// <param name="today">The trust's today.</param>
    public bool IsCertificateDueOn(DateOnly today) => today >= DueOn.AddDays(-DueWindowDays);

    /// <summary>Gets whether the current certificate falls short of the lease.</summary>
    public bool IsShort => Check is { MeetsLease: false };

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return LeaseStartDate;
        yield return Current as object ?? string.Empty;
        yield return Check as object ?? string.Empty;
    }
}
