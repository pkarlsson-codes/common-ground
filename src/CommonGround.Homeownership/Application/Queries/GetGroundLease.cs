using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns a single ground lease by ID.</summary>
public sealed record GetGroundLease(GroundLeaseId GroundLeaseId);

/// <summary>
/// A ground lease, the trust date it was recorded on (for a lease that predates CommonGround, later than its start
/// date), and what it owes in lease fees as of the trust's today.
/// </summary>
public sealed record GroundLeaseDetails(GroundLeaseView Lease, DateOnly RecordedOn, LeaseFeeStatement LeaseFees);

/// <summary>Handles <see cref="GetGroundLease"/>.</summary>
public static class GetGroundLeaseHandler
{
    /// <summary>Loads the ground lease view by ID, throwing if not found.</summary>
    public static async Task<GroundLeaseDetails> Handle(
        GetGroundLease request,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var view = await session.LoadAsync<GroundLeaseView>(request.GroundLeaseId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.GroundLeaseId.Value);

        var recordedOn = await trustClock.ToTrustDateAsync(session.TenantId, view.CreatedAt, cancellationToken).ConfigureAwait(false);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new GroundLeaseDetails(view, recordedOn, LeaseFeeStatements.For(view, today));
    }
}
