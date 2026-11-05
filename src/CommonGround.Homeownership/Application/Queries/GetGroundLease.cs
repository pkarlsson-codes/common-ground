using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns a single ground lease by ID.</summary>
public sealed record GetGroundLease(GroundLeaseId GroundLeaseId);

/// <summary>
/// A ground lease and the trust date it was recorded on, which for a lease that predates CommonGround is
/// later than its start date.
/// </summary>
public sealed record GroundLeaseDetails(GroundLeaseView Lease, DateOnly RecordedOn);

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

        return new GroundLeaseDetails(view, recordedOn);
    }
}
