using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.MortgageRequests;

/// <summary>
/// The terms of a requested loan that the homeowner sends the trust (Model Lease Art 8.3 a–g): lender, purpose,
/// amount, the total debt it leaves on the home, rate and term.
/// </summary>
public sealed class MortgageLoanTerms : ValueObject
{
    /// <summary>The longest term a loan can have, in years.</summary>
    public const int MaxTermYears = 50;

    /// <summary>The interest rate, in percent, a loan's rate must be below.</summary>
    public const decimal InterestRateLimitPercent = 100m;

    private MortgageLoanTerms(
        string lender,
        MortgagePurpose purpose,
        Money principal,
        Money totalDebtAfterClosing,
        decimal interestRatePercent,
        MortgageRateType rateType,
        int termYears)
    {
        Lender = lender;
        Purpose = purpose;
        Principal = principal;
        TotalDebtAfterClosing = totalDebtAfterClosing;
        InterestRatePercent = interestRatePercent;
        RateType = rateType;
        TermYears = termYears;
    }

    /// <summary>
    /// Creates the terms. Refused without a lender, for a principal of zero or less, a total debt below the principal,
    /// a rate below zero or from 100%, or a term outside 1 to 50 years.
    /// </summary>
    public static MortgageLoanTerms Create(
        string lender,
        MortgagePurpose purpose,
        Money principal,
        Money totalDebtAfterClosing,
        decimal interestRatePercent,
        MortgageRateType rateType,
        int termYears)
    {
        ArgumentNullException.ThrowIfNull(purpose);
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(totalDebtAfterClosing);
        ArgumentNullException.ThrowIfNull(rateType);

        if (string.IsNullOrWhiteSpace(lender))
            throw BusinessRuleViolation.MortgageLoanTerms.Create.LenderRequired();

        if (principal <= Money.Zero)
            throw BusinessRuleViolation.MortgageLoanTerms.Create.PrincipalNonPositive(principal.Amount);

        if (totalDebtAfterClosing < principal)
            throw BusinessRuleViolation.MortgageLoanTerms.Create.TotalDebtBelowPrincipal(totalDebtAfterClosing.Amount, principal.Amount);

        if (interestRatePercent is < 0m or >= InterestRateLimitPercent)
            throw BusinessRuleViolation.MortgageLoanTerms.Create.InterestRateOutOfRange(interestRatePercent);

        if (termYears is < 1 or > MaxTermYears)
            throw BusinessRuleViolation.MortgageLoanTerms.Create.TermYearsOutOfRange(termYears);

        return new MortgageLoanTerms(lender.Trim(), purpose, principal, totalDebtAfterClosing, interestRatePercent, rateType, termYears);
    }

    /// <summary>Gets the lender making the loan.</summary>
    public string Lender { get; }

    /// <summary>Gets what the loan is for.</summary>
    public MortgagePurpose Purpose { get; }

    /// <summary>Gets the loan amount.</summary>
    public Money Principal { get; }

    /// <summary>Gets all mortgage debt on the home once the loan closes, the new loan included.</summary>
    public Money TotalDebtAfterClosing { get; }

    /// <summary>Gets the yearly interest rate, in percent.</summary>
    public decimal InterestRatePercent { get; }

    /// <summary>Gets whether the rate is fixed or adjustable.</summary>
    public MortgageRateType RateType { get; }

    /// <summary>Gets the term in years.</summary>
    public int TermYears { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Lender;
        yield return Purpose;
        yield return Principal;
        yield return TotalDebtAfterClosing;
        yield return InterestRatePercent;
        yield return RateType;
        yield return TermYears;
    }
}
