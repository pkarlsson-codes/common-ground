using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns a resale price estimate for a ground lease given runtime market inputs.</summary>
public sealed record GetResaleEstimate(
    GroundLeaseId GroundLeaseId,
    decimal OriginalPurchasePrice,
    decimal CurrentAppraisalValue,
    decimal OriginalAppraisalValue,
    int OwnershipYears,
    decimal? ApprovedImprovements,
    decimal? CurrentIndexValue);

/// <summary>Handles <see cref="GetResaleEstimate"/>.</summary>
public static class GetResaleEstimateHandler
{
    /// <summary>
    /// Loads the ground lease view, reconstructs its resale policy terms, and delegates to
    /// <see cref="ResalePriceCalculator"/> to produce the maximum allowable resale price.
    /// </summary>
    public static async Task<decimal> Handle(
        GetResaleEstimate request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<ReadModels.GroundLeaseView>(request.GroundLeaseId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.GroundLeaseId.Value);

        var terms = ResalePolicyTerms.FromString(
            view.FormulaKind,
            view.AppreciationShare,
            view.IndexType,
            view.BaselineIndexValue,
            view.AnnualPercentage,
            view.ImprovementsCap);

        var input = new ResaleCalculationInput(
            request.OriginalPurchasePrice,
            request.CurrentAppraisalValue,
            request.OriginalAppraisalValue,
            request.OwnershipYears,
            request.ApprovedImprovements,
            request.CurrentIndexValue);

        return ResalePriceCalculator.Calculate(terms, input);
    }
}
