using CommonGround.Homeownership.Contracts.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Stewardship.Domain.StewardshipCases;
using CommonGround.Stewardship.Infrastructure.Persistence;
using Marten;

namespace CommonGround.Stewardship.Application.EventHandlers;

/// <summary>Closes the active StewardshipCase when a ground lease is terminated in the Homeownership BC.</summary>
public sealed class GroundLeaseTerminatedHandler(IDocumentSession session)
{
    /// <summary>Handles the integration event.</summary>
    public async Task Handle(GroundLeaseTerminated @event, CancellationToken ct)
    {
        var view = await session.Query<StewardshipCaseView>()
            .Where(v => v.GroundLeaseId == @event.GroundLeaseId && v.Status == nameof(StewardshipCaseStatus.Active))
            .FirstOrDefaultAsync(ct).ConfigureAwait(false);

        if (view is null)
        {
            // No active case found - most likely a redelivery of this event after the case was
            // already closed. Treat as a no-op rather than an error.
            return;
        }

        var stream = await session.Events.FetchForWriting<StewardshipCase>(view.Id, ct).ConfigureAwait(false);
        var sc = stream.Aggregate ?? throw new AggregateNotFoundException(view.Id);

        sc.Close(@event.Reason, @event.TerminatedDate, UserId.Create(@event.TerminatedBy));
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}
