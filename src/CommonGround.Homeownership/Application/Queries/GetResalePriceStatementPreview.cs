using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Works out a lease's resale price statement without recording it.</summary>
/// <param name="GroundLeaseId">The lease.</param>
/// <param name="StatementDate">The day to work the price out on.</param>
/// <param name="Figures">The figures the formula needs that the lease doesn't hold.</param>
public sealed record GetResalePriceStatementPreview(
    GroundLeaseId GroundLeaseId,
    DateOnly StatementDate,
    ResalePriceStatementFigures Figures);

/// <summary>Handles <see cref="GetResalePriceStatementPreview"/>.</summary>
public static class GetResalePriceStatementPreviewHandler
{
    /// <summary>Prices the statement from the lease itself, with the same rules as recording it.</summary>
    public static async Task<ResalePriceStatementPrice> Handle(
        GetResalePriceStatementPreview request,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var lease = await session.Events
            .AggregateStreamAsync<GroundLease>(request.GroundLeaseId.Value, token: cancellationToken)
            .ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.GroundLeaseId.Value);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return lease.ResalePriceStatementPriceOn(request.StatementDate, request.Figures, today);
    }
}
