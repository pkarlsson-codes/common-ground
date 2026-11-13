using CommonGround.Homeownership.Application.Errors;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;

/// <summary>
/// The figures staff give for a resale price statement that the lease doesn't hold. Which ones count depends on the
/// lease's formula; the rest are ignored.
/// </summary>
/// <param name="MarketValue">An appraisal-based lease's estimated market value on the statement date.</param>
/// <param name="MarketValueSource">Where the market value came from, such as "County assessment 2026".</param>
/// <param name="IndexValue">An index-based lease's index value on the statement date.</param>
/// <param name="ImprovementsCredit">Approved improvements, for a lease with an improvements cap.</param>
public sealed record ResalePriceStatementFigures(
    decimal? MarketValue,
    string? MarketValueSource,
    decimal? IndexValue,
    decimal? ImprovementsCredit);

/// <summary>
/// The most a home may sell for on a statement date under its lease's formula (Ch 23, Disclosure), with the figures
/// used. Only the figures the formula used are kept; the improvements credit is the one counted, after the cap.
/// </summary>
public sealed record ResalePriceStatementPrice(
    DateOnly StatementDate,
    int YearsOwned,
    Money? MarketValue,
    string? MarketValueSource,
    decimal? IndexValue,
    Money? ImprovementsCredit,
    Money Price)
{
    /// <summary>
    /// Works out the price with the same formula as a resale appraisal. Refused for a statement date in the future or
    /// before the lease start; for an appraisal-based lease, without an original appraised value or without a market
    /// value above zero and its source; for an index-based lease, without an index value above zero; and for a
    /// negative improvements credit.
    /// </summary>
    public static ResalePriceStatementPrice Calculate(
        ResalePolicyTerms terms,
        decimal basePrice,
        decimal? originalAppraisedValue,
        DateOnly leaseStartDate,
        DateOnly statementDate,
        DateOnly today,
        ResalePriceStatementFigures figures)
    {
        ArgumentNullException.ThrowIfNull(terms);
        ArgumentNullException.ThrowIfNull(figures);

        if (statementDate > today)
            throw BusinessRuleViolation.GroundLease.RecordResalePriceStatement.StatementDateInFuture(statementDate, today);

        if (statementDate < leaseStartDate)
            throw BusinessRuleViolation.GroundLease.RecordResalePriceStatement.StatementDateBeforeLeaseStart(statementDate, leaseStartDate);

        Money? marketValue = null;
        string? marketValueSource = null;
        decimal? indexValue = null;

        if (terms.FormulaKind == FormulaKind.AppraisalBased)
        {
            if (originalAppraisedValue is null)
                throw BusinessRuleViolation.GroundLease.RecordResalePriceStatement.OriginalAppraisedValueMissing();

            if (figures.MarketValue is not { } value)
                throw BusinessRuleViolation.GroundLease.RecordResalePriceStatement.MarketValueRequired();

            if (value <= 0)
                throw BusinessRuleViolation.GroundLease.RecordResalePriceStatement.MarketValueNonPositive(value);

            if (string.IsNullOrWhiteSpace(figures.MarketValueSource))
                throw BusinessRuleViolation.GroundLease.RecordResalePriceStatement.MarketValueSourceRequired();

            marketValue = Money.Create(value);
            marketValueSource = figures.MarketValueSource.Trim();
        }
        else if (terms.FormulaKind == FormulaKind.IndexBased)
        {
            if (figures.IndexValue is not { } value)
                throw BusinessRuleViolation.GroundLease.RecordResalePriceStatement.IndexValueRequired();

            if (value <= 0)
                throw BusinessRuleViolation.GroundLease.RecordResalePriceStatement.IndexValueNonPositive(value);

            indexValue = value;
        }

        if (figures.ImprovementsCredit < 0)
            throw BusinessRuleViolation.GroundLease.RecordResalePriceStatement.ImprovementsCreditNegative(figures.ImprovementsCredit.Value);

        Money? improvementsCredit = terms.ImprovementsCap is { } cap && figures.ImprovementsCredit is { } credit and > 0
            ? Money.Create(Math.Min(credit, cap))
            : null;

        var yearsOwned = ResalePrice.WholeYears(leaseStartDate, statementDate);
        var input = new ResaleCalculationInput(
            basePrice,
            marketValue?.Amount ?? 0,
            originalAppraisedValue ?? 0,
            yearsOwned,
            improvementsCredit?.Amount,
            indexValue);

        return new ResalePriceStatementPrice(
            statementDate,
            yearsOwned,
            marketValue,
            marketValueSource,
            indexValue,
            improvementsCredit,
            Money.RoundedFrom(ResalePriceCalculator.Calculate(terms, input)));
    }

    /// <summary>
    /// The day a lease's next yearly statement is due: a year after the last statement's date, or after the lease
    /// start when none was sent.
    /// </summary>
    public static DateOnly NextDueOn(DateOnly leaseStartDate, DateOnly? lastStatementDate) =>
        (lastStatementDate ?? leaseStartDate).AddYears(1);
}
