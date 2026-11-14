using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.ImprovementRequests;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns a lease's improvement requests, latest received first.</summary>
/// <param name="GroundLeaseId">The lease.</param>
public sealed record GetImprovementRequests(GroundLeaseId GroundLeaseId);

/// <summary>A lease's improvement requests. Wrapped rather than returned as a bare list, which Wolverine would take for cascading messages.</summary>
/// <param name="Requests">The requests, latest received first.</param>
public sealed record ImprovementRequestList(IReadOnlyList<ImprovementRequestView> Requests);

/// <summary>Handles <see cref="GetImprovementRequests"/>.</summary>
public static class GetImprovementRequestsHandler
{
    /// <summary>Loads the lease's requests.</summary>
    public static async Task<ImprovementRequestList> Handle(GetImprovementRequests request, IQuerySession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);

        var leaseId = request.GroundLeaseId.Value;
        var requests = await session.Query<ImprovementRequestView>()
            .Where(v => v.GroundLeaseId == leaseId)
            .OrderByDescending(v => v.ReceivedOn)
            .ThenByDescending(v => v.RecordedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new ImprovementRequestList(requests);
    }
}

/// <summary>Returns one improvement request.</summary>
/// <param name="ImprovementRequestId">The request.</param>
public sealed record GetImprovementRequest(ImprovementRequestId ImprovementRequestId);

/// <summary>Handles <see cref="GetImprovementRequest"/>.</summary>
public static class GetImprovementRequestHandler
{
    /// <summary>Loads the request.</summary>
    public static async Task<ImprovementRequestView> Handle(GetImprovementRequest request, IQuerySession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);

        return await session.LoadAsync<ImprovementRequestView>(request.ImprovementRequestId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(typeof(ImprovementRequestView), request.ImprovementRequestId.Value);
    }
}
