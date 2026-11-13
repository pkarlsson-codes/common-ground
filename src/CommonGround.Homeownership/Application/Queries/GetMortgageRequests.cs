using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.MortgageRequests;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns a lease's mortgage requests, latest received first.</summary>
/// <param name="GroundLeaseId">The lease.</param>
public sealed record GetMortgageRequests(GroundLeaseId GroundLeaseId);

/// <summary>A lease's mortgage requests. Wrapped rather than returned as a bare list, which Wolverine would take for cascading messages.</summary>
/// <param name="Requests">The requests, latest received first.</param>
public sealed record MortgageRequestList(IReadOnlyList<MortgageRequestView> Requests);

/// <summary>Handles <see cref="GetMortgageRequests"/>.</summary>
public static class GetMortgageRequestsHandler
{
    /// <summary>Loads the lease's requests.</summary>
    public static async Task<MortgageRequestList> Handle(GetMortgageRequests request, IQuerySession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);

        var leaseId = request.GroundLeaseId.Value;
        var requests = await session.Query<MortgageRequestView>()
            .Where(v => v.GroundLeaseId == leaseId)
            .OrderByDescending(v => v.ReceivedOn)
            .ThenByDescending(v => v.RecordedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new MortgageRequestList(requests);
    }
}

/// <summary>Returns one mortgage request.</summary>
/// <param name="MortgageRequestId">The request.</param>
public sealed record GetMortgageRequest(MortgageRequestId MortgageRequestId);

/// <summary>Handles <see cref="GetMortgageRequest"/>.</summary>
public static class GetMortgageRequestHandler
{
    /// <summary>Loads the request.</summary>
    public static async Task<MortgageRequestView> Handle(GetMortgageRequest request, IQuerySession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);

        return await session.LoadAsync<MortgageRequestView>(request.MortgageRequestId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(typeof(MortgageRequestView), request.MortgageRequestId.Value);
    }
}
