using CommonGround.Homeownership.Contracts.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using CommonGround.Stewardship.Domain.StewardshipCases;
using CommonGround.Stewardship.Infrastructure.Persistence;
using JasperFx.Events;
using Marten;

namespace CommonGround.Stewardship.Application.EventHandlers;

/// <summary>Closes the active StewardshipCase when a ground lease is terminated in the Homeownership BC.</summary>
public static class GroundLeaseTerminatedHandler
{
    /// <summary>
    /// Fetches the lease's active case for writing. There is none when this event is redelivered after
    /// the case was already closed.
    /// </summary>
    public static async Task<IEventStream<StewardshipCase>?> LoadAsync(
        GroundLeaseTerminated @event,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);

        var activeStatus = nameof(StewardshipCaseStatus.Active);
        var caseId = await session.Query<StewardshipCaseView>()
            .Where(v => v.GroundLeaseId == @event.GroundLeaseId && v.Status == activeStatus)
            .Select(v => v.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (caseId is null)
            return null;

        return await session.Events.FetchForWriting<StewardshipCase>(caseId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Closes the active case, if there is one.</summary>
    public static void Handle(GroundLeaseTerminated @event, IEventStream<StewardshipCase>? stream)
    {
        ArgumentNullException.ThrowIfNull(@event);

        stream?.Decide(sc => sc.Close(@event.Reason, @event.TerminatedDate, UserId.Create(@event.TerminatedBy)));
    }
}
