using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;

/// <summary>
/// Defines the terms of the resale policy for a ground lease, including the formula kind and any parameters needed for that formula.
/// </summary>
public sealed class ResalePolicyTerms : ValueObject
{
    /// <summary>
    /// The kind of resale formula used to calculate the resale price of a home under a ground lease.
    /// </summary>
    public FormulaKind FormulaKind { get; }

    /// <summary>
    /// The share of appreciation in the home's value that the ground lease holder is entitled to upon resale.
    /// </summary>
    public decimal? AppreciationShare { get; }

    /// <summary>
    /// The type of index used for index-based resale formulas (e.g., "CPI", "HPI"), if applicable.
    /// </summary>
    public string? IndexType { get; }

    /// <summary>
    /// The baseline value of the index at the time of the original purchase, used for index-based resale formulas.
    /// </summary>
    public decimal? BaselineIndexValue { get; }

    /// <summary>
    /// The fixed annual percentage increase applied to the original purchase price for fixed percentage resale formulas.
    /// </summary>
    public decimal? AnnualPercentage { get; }

    /// <summary>
    /// An optional cap on the amount of appreciation that can be claimed for improvements made by the homeowner, regardless of the formula used to calculate the resale price.
    /// </summary>
    public decimal? ImprovementsCap { get; }

    private ResalePolicyTerms(
        FormulaKind formulaKind,
        decimal? appreciationShare,
        string? indexType,
        decimal? baselineIndexValue,
        decimal? annualPercentage,
        decimal? improvementsCap)
    {
        FormulaKind = formulaKind;
        AppreciationShare = appreciationShare;
        IndexType = indexType;
        BaselineIndexValue = baselineIndexValue;
        AnnualPercentage = annualPercentage;
        ImprovementsCap = improvementsCap;
    }

    /// <summary>
    /// Factory method to create resale policy terms for an appraisal-based formula. Validates that the appreciation share is between 0 and 1, and that any specified improvements cap is positive.
    /// </summary>
    /// <param name="appreciationShare">The share of appreciation in the home's value that the ground lease holder is entitled to upon resale.</param>
    /// <param name="improvementsCap">An optional cap on the amount of appreciation that can be claimed for improvements made by the homeowner.</param>
    /// <returns>A new instance of <see cref="ResalePolicyTerms"/> configured for an appraisal-based formula.</returns>
    /// <exception cref="BusinessRuleViolationException">Thrown when the appreciation share is not between 0 and 1, or when the improvements cap is not positive.</exception>
    public static ResalePolicyTerms AppraisalBased(decimal appreciationShare, decimal? improvementsCap = null)
    {
        if (appreciationShare < 0.0m || appreciationShare > 1.0m)
            throw BusinessRuleViolation.ResalePolicyTerms.AppraisalBased.AppreciationShareOutOfRange(appreciationShare);

        ValidateImprovementsCap(improvementsCap);

        return new ResalePolicyTerms(
            FormulaKind.AppraisalBased,
            appreciationShare: appreciationShare,
            indexType: null,
            baselineIndexValue: null,
            annualPercentage: null,
            improvementsCap: improvementsCap);
    }

    /// <summary>
    /// Factory method to create resale policy terms for an index-based formula. Validates that the index type is specified, the baseline index value is positive, and any specified improvements cap is positive.
    /// </summary>
    /// <param name="indexType">The type of index used for index-based resale formulas (e.g., "CPI", "HPI").</param>
    /// <param name="baselineIndexValue">The baseline value of the index at the time of the original purchase.</param>
    /// <param name="improvementsCap">An optional cap on the amount of appreciation that can be claimed for improvements made by the homeowner.</param>
    /// <returns>A new instance of <see cref="ResalePolicyTerms"/> configured for an index-based formula.</returns>
    /// <exception cref="BusinessRuleViolationException">Thrown when the index type is not specified, the baseline index value is not positive, or the improvements cap is not positive.</exception>
    public static ResalePolicyTerms IndexBased(
        string indexType, decimal baselineIndexValue, decimal? improvementsCap = null)
    {
        if (string.IsNullOrWhiteSpace(indexType))
            throw BusinessRuleViolation.ResalePolicyTerms.IndexBased.IndexTypeRequired();

        if (baselineIndexValue <= 0)
            throw BusinessRuleViolation.ResalePolicyTerms.IndexBased.BaselineIndexNonPositive(baselineIndexValue);

        ValidateImprovementsCap(improvementsCap);

        return new ResalePolicyTerms(
            FormulaKind.IndexBased,
            appreciationShare: null,
            indexType: indexType,
            baselineIndexValue: baselineIndexValue,
            annualPercentage: null,
            improvementsCap: improvementsCap);
    }

    /// <summary>
    /// Factory method to create resale policy terms for a fixed percentage formula. Validates that the annual percentage is between 0 and 1, and any specified improvements cap is positive.
    /// </summary>
    /// <param name="annualPercentage">The fixed annual percentage used for the resale formula.</param>
    /// <param name="improvementsCap">An optional cap on the amount of appreciation that can be claimed for improvements made by the homeowner.</param>
    /// <returns>A new instance of <see cref="ResalePolicyTerms"/> configured for a fixed percentage formula.</returns>
    /// <exception cref="BusinessRuleViolationException">Thrown when the annual percentage is not between 0 and 1, or when the improvements cap is not positive.</exception>
    public static ResalePolicyTerms FixedPercentage(decimal annualPercentage, decimal? improvementsCap = null)
    {
        if (annualPercentage < 0.0m || annualPercentage > 1.0m)
            throw BusinessRuleViolation.ResalePolicyTerms.FixedPercentage.AnnualPercentageOutOfRange(annualPercentage);

        ValidateImprovementsCap(improvementsCap);

        return new ResalePolicyTerms(
            FormulaKind.FixedPercentage,
            appreciationShare: null,
            indexType: null,
            baselineIndexValue: null,
            annualPercentage: annualPercentage,
            improvementsCap: improvementsCap);
    }

    /// <summary>
    /// Factory method that reconstructs <see cref="ResalePolicyTerms"/> from a stored formula kind string and its
    /// associated parameters. Use this when restoring from a read model or mapping user-supplied input that has
    /// already been validated upstream.
    /// </summary>
    /// <param name="formulaKind">The formula kind string, e.g. "AppraisalBased", "IndexBased", or "FixedPercentage".</param>
    /// <param name="appreciationShare">Required for AppraisalBased formulas.</param>
    /// <param name="indexType">Required for IndexBased formulas.</param>
    /// <param name="baselineIndexValue">Required for IndexBased formulas.</param>
    /// <param name="annualPercentage">Required for FixedPercentage formulas.</param>
    /// <param name="improvementsCap">Optional cap on improvements appreciation for any formula kind.</param>
    /// <returns>A <see cref="ResalePolicyTerms"/> instance for the specified formula kind.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="formulaKind"/> is not a recognised value.</exception>
    public static ResalePolicyTerms FromString(
        string formulaKind,
        decimal? appreciationShare,
        string? indexType,
        decimal? baselineIndexValue,
        decimal? annualPercentage,
        decimal? improvementsCap)
        => formulaKind switch
        {
            "AppraisalBased" => AppraisalBased(appreciationShare!.Value, improvementsCap),
            "IndexBased" => IndexBased(indexType!, baselineIndexValue!.Value, improvementsCap),
            "FixedPercentage" => FixedPercentage(annualPercentage!.Value, improvementsCap),
            _ => throw new InvalidOperationException($"Unknown formula kind: {formulaKind}")
        };

    private static void ValidateImprovementsCap(decimal? cap)
    {
        if (cap is <= 0)
            throw BusinessRuleViolation.ResalePolicyTerms.Create.ImprovementsCapNonPositive(cap);
    }

    /// <inheritdoc />
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FormulaKind;
        if (AppreciationShare.HasValue) yield return AppreciationShare.Value;
        if (IndexType is not null) yield return IndexType;
        if (BaselineIndexValue.HasValue) yield return BaselineIndexValue.Value;
        if (AnnualPercentage.HasValue) yield return AnnualPercentage.Value;
        if (ImprovementsCap.HasValue) yield return ImprovementsCap.Value;
    }
}