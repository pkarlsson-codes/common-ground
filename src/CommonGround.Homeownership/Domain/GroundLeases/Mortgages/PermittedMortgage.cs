using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.Mortgages;

/// <summary>
/// A loan secured on the home that the trust permitted (Model Lease Art 8.1). The lender gets a copy of each notice
/// of default while the loan isn't paid off. A paid-off mortgage stays on the lease for the record.
/// </summary>
public sealed class PermittedMortgage : Entity<PermittedMortgageId>
{
    private PermittedMortgage(
        PermittedMortgageId id,
        string lender,
        string? noticeAddress,
        Money originalPrincipal,
        DateOnly permittedOn,
        MortgagePermissionSource source)
    {
        Id = id;
        Lender = lender;
        NoticeAddress = noticeAddress;
        OriginalPrincipal = originalPrincipal;
        PermittedOn = permittedOn;
        Source = source;
    }

    /// <summary>Creates a mortgage from its recorded facts. The lease checks the rules before deciding on it.</summary>
    /// <param name="id">The mortgage's ID.</param>
    /// <param name="lender">The lender or servicer.</param>
    /// <param name="noticeAddress">Where the trust sends copies of notices, if known.</param>
    /// <param name="originalPrincipal">The loan amount when it closed.</param>
    /// <param name="permittedOn">The day the trust permitted it.</param>
    /// <param name="source">How the trust permitted it.</param>
    public static PermittedMortgage Create(
        PermittedMortgageId id,
        string lender,
        string? noticeAddress,
        Money originalPrincipal,
        DateOnly permittedOn,
        MortgagePermissionSource source)
    {
        ArgumentNullException.ThrowIfNull(originalPrincipal);
        ArgumentNullException.ThrowIfNull(source);

        return new PermittedMortgage(id, lender, noticeAddress, originalPrincipal, permittedOn, source);
    }

    /// <summary>Gets the lender or servicer.</summary>
    public string Lender { get; private set; }

    /// <summary>Gets where the trust sends copies of notices, or null when not on file.</summary>
    public string? NoticeAddress { get; private set; }

    /// <summary>Gets the loan amount when it closed.</summary>
    public Money OriginalPrincipal { get; }

    /// <summary>Gets the day the trust permitted it.</summary>
    public DateOnly PermittedOn { get; }

    /// <summary>Gets how the trust permitted it.</summary>
    public MortgagePermissionSource Source { get; }

    /// <summary>Gets the day it was paid off, or null while it is current.</summary>
    public DateOnly? PaidOffOn { get; private set; }

    /// <summary>Gets whether it is paid off.</summary>
    public bool IsPaidOff => PaidOffOn is not null;

    /// <summary>Gets whether it was permitted and not yet paid off on a day.</summary>
    /// <param name="day">The day to check.</param>
    public bool IsCurrentOn(DateOnly day) => PermittedOn <= day && (PaidOffOn is not { } paidOff || paidOff > day);

    /// <summary>Changes the lender and notice address. Called only from the lease's Apply.</summary>
    internal void ChangeLender(string lender, string? noticeAddress)
    {
        Lender = lender;
        NoticeAddress = noticeAddress;
    }

    /// <summary>Marks it paid off. Called only from the lease's Apply.</summary>
    internal void MarkPaidOff(DateOnly paidOffOn) => PaidOffOn = paidOffOn;
}
