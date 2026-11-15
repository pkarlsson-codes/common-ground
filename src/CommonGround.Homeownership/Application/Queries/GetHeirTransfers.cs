using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Application.Services;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.HeirTransfers;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns a lease's heir transfers, latest opened first.</summary>
/// <param name="GroundLeaseId">The lease.</param>
public sealed record GetHeirTransfers(GroundLeaseId GroundLeaseId);

/// <summary>A lease's heir transfers. Wrapped rather than returned as a bare list, which Wolverine would take for cascading messages.</summary>
/// <param name="Transfers">The transfers, latest opened first.</param>
public sealed record HeirTransferList(IReadOnlyList<HeirTransferView> Transfers);

/// <summary>Handles <see cref="GetHeirTransfers"/>.</summary>
public static class GetHeirTransfersHandler
{
    /// <summary>Loads the lease's transfers.</summary>
    public static async Task<HeirTransferList> Handle(GetHeirTransfers request, IQuerySession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);

        var leaseId = request.GroundLeaseId.Value;
        var transfers = await session.Query<HeirTransferView>()
            .Where(v => v.GroundLeaseId == leaseId)
            .OrderByDescending(v => v.OpenedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new HeirTransferList(transfers);
    }
}

/// <summary>Returns one heir transfer.</summary>
/// <param name="HeirTransferId">The transfer.</param>
public sealed record GetHeirTransfer(HeirTransferId HeirTransferId);

/// <summary>Handles <see cref="GetHeirTransfer"/>.</summary>
public static class GetHeirTransferHandler
{
    /// <summary>Loads the transfer.</summary>
    public static async Task<HeirTransferView> Handle(GetHeirTransfer request, IQuerySession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);

        return await session.LoadAsync<HeirTransferView>(request.HeirTransferId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(typeof(HeirTransferView), request.HeirTransferId.Value);
    }
}

/// <summary>Returns the income limit a transfer's heir is judged by, when the heir is not family.</summary>
/// <param name="HeirTransferId">The transfer.</param>
public sealed record GetHeirIncomeLimit(HeirTransferId HeirTransferId);

/// <summary>The income limits of a transfer's home, from the schedule in effect on the notice date.</summary>
/// <param name="Limit">The home's limit and schedule, or null when the home has no income limit.</param>
public sealed record HeirIncomeLimitResult(HeirIncomeLimit? Limit);

/// <summary>Handles <see cref="GetHeirIncomeLimit"/>.</summary>
public static class GetHeirIncomeLimitHandler
{
    /// <summary>Looks up the home's limit on the transfer's notice date.</summary>
    public static async Task<HeirIncomeLimitResult> Handle(
        GetHeirIncomeLimit request, IQuerySession session, IHomeIncomeLimits incomeLimits, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(incomeLimits);

        var transfer = await session.LoadAsync<HeirTransferView>(request.HeirTransferId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(typeof(HeirTransferView), request.HeirTransferId.Value);
        var limit = await incomeLimits.FindAsync(session, transfer.HomeId, transfer.NoticeReceivedOn, cancellationToken).ConfigureAwait(false);

        return new HeirIncomeLimitResult(limit);
    }
}
