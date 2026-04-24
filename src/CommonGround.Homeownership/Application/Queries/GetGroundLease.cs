using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns a single ground lease by ID.</summary>
public sealed record GetGroundLease(GroundLeaseId GroundLeaseId);

/// <summary>Handles <see cref="GetGroundLease"/>.</summary>
public static class GetGroundLeaseHandler
{
    /// <summary>Loads the ground lease view by ID, throwing if not found.</summary>
    public static async Task<GroundLeaseView> Handle(
        GetGroundLease request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<GroundLeaseView>(request.GroundLeaseId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.GroundLeaseId.Value);

        return view;
    }
}
