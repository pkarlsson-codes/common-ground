using CommonGround.Homeownership.Application.Errors;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Works out the prices a resale appraisal would set, without recording it.</summary>
public sealed record GetResalePricePreview(
    ResaleId ResaleId,
    Money AppraisedValue,
    DateOnly AppraisalDate,
    Money? ApprovedImprovements,
    decimal? CurrentIndexValue);

/// <summary>Handles <see cref="GetResalePricePreview"/>.</summary>
public static class GetResalePricePreviewHandler
{
    /// <summary>Prices the home from the resale's lease, with the same rules as recording the appraisal.</summary>
    public static async Task<ResalePrice> Handle(
        GetResalePricePreview request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);

        var resale = await session.LoadAsync<ResaleView>(request.ResaleId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.ResaleId.Value);
        var lease = await session.LoadAsync<GroundLeaseView>(resale.GroundLeaseId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(resale.GroundLeaseId);

        if (lease.OriginalAppraisedValue is not { } originalAppraisedValue)
            throw BusinessRuleViolation.Resale.RecordAppraisal.LeaseFiguresMissing();

        if (!request.AppraisedValue.IsPositive)
            throw BusinessRuleViolation.Resale.RecordAppraisal.AppraisedValueNonPositive(request.AppraisedValue.Amount);

        var terms = ResalePolicyTerms.FromString(
            lease.FormulaKind,
            lease.AppreciationShare,
            lease.IndexType,
            lease.BaselineIndexValue,
            lease.AnnualPercentage,
            lease.ImprovementsCap);

        return ResalePrice.Calculate(
            terms,
            Money.Create(lease.BasePrice),
            Money.Create(originalAppraisedValue),
            lease.LeaseStartDate,
            request.AppraisedValue,
            request.AppraisalDate,
            request.ApprovedImprovements,
            request.CurrentIndexValue);
    }
}
