using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.MortgageRequests;

/// <summary>
/// The appraisal the lender ordered for a requested loan. The trust uses it to work out today's purchase option
/// price for the debt check.
/// </summary>
public sealed class LoanAppraisal : ValueObject
{
    private LoanAppraisal(Money appraisedValue, DateOnly appraisalDate, decimal? currentIndexValue)
    {
        AppraisedValue = appraisedValue;
        AppraisalDate = appraisalDate;
        CurrentIndexValue = currentIndexValue;
    }

    /// <summary>Creates the appraisal. Refused for a value of zero or less, or an index value of zero or less.</summary>
    /// <param name="appraisedValue">The home's appraised value.</param>
    /// <param name="appraisalDate">The day of the appraisal.</param>
    /// <param name="currentIndexValue">The index value on that day, when the lease's formula is index based.</param>
    public static LoanAppraisal Create(Money appraisedValue, DateOnly appraisalDate, decimal? currentIndexValue)
    {
        ArgumentNullException.ThrowIfNull(appraisedValue);

        if (appraisedValue <= Money.Zero)
            throw BusinessRuleViolation.LoanAppraisal.Create.AppraisedValueNonPositive(appraisedValue.Amount);

        if (currentIndexValue <= 0m)
            throw BusinessRuleViolation.LoanAppraisal.Create.CurrentIndexValueNonPositive(currentIndexValue.Value);

        return new LoanAppraisal(appraisedValue, appraisalDate, currentIndexValue);
    }

    /// <summary>Gets the home's appraised value.</summary>
    public Money AppraisedValue { get; }

    /// <summary>Gets the day of the appraisal.</summary>
    public DateOnly AppraisalDate { get; }

    /// <summary>Gets the index value on the appraisal date, when the lease's formula is index based.</summary>
    public decimal? CurrentIndexValue { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AppraisedValue;
        yield return AppraisalDate;
        yield return CurrentIndexValue ?? 0m;
    }
}
