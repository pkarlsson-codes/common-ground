using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.Stewardship.Domain.StewardshipCases;
using CommonGround.Stewardship.Infrastructure.Persistence;
using Marten;

namespace CommonGround.API.Stewardship.Consumers;

#pragma warning disable SA1600
#pragma warning disable CA2007
public sealed class GroundLeaseTerminatedConsumer(IDocumentSession session)
{
    public async Task Handle(GroundLeaseTerminated @event, CancellationToken ct)
    {
        var view = await session.Query<StewardshipCaseView>()
            .Where(v => v.GroundLeaseId == @event.AggregateId && v.Status == nameof(StewardshipCaseStatus.Active))
            .FirstOrDefaultAsync(ct);

        if (view is null)
        {
            throw new InvalidOperationException($"No active StewardshipCase found for ground lease {@event.AggregateId}.");
        }

        var stream = await session.Events.FetchForWriting<StewardshipCase>(view.Id, ct);
        var sc = stream.Aggregate ?? throw new InvalidOperationException($"StewardshipCase stream {view.Id} not found.");
        sc.Close(@event.Reason, @event.TerminatedDate, UserId.Create(@event.TerminatedBy));
        sc.ValidateInvariants();
        stream.AppendMany(sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}
