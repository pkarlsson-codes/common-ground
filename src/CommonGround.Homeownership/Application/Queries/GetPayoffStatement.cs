using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Works out what a resale's seller owes the trust as of a day, without recording it.</summary>
/// <param name="ResaleId">The resale.</param>
/// <param name="GoodThrough">The day the figures are worked out for.</param>
public sealed record GetPayoffStatement(ResaleId ResaleId, DateOnly GoodThrough);

/// <summary>Handles <see cref="GetPayoffStatement"/>.</summary>
public static class GetPayoffStatementHandler
{
    /// <summary>Works the statement out from the lease's own fee statement, with the same rules as recording it.</summary>
    public static async Task<PayoffStatement> Handle(
        GetPayoffStatement request,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var resale = await session.Events
            .AggregateStreamAsync<Resale>(request.ResaleId.Value, token: cancellationToken)
            .ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.ResaleId.Value);
        var lease = await session.Events
            .AggregateStreamAsync<GroundLease>(resale.GroundLeaseId.Value, token: cancellationToken)
            .ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(resale.GroundLeaseId.Value);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return resale.PayoffStatementOn(lease, request.GoodThrough, today);
    }
}
