using CommonGround.Homeownership.Domain.GroundLeases.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.Stewardship.Domain.StewardshipCases;
using Marten;

namespace CommonGround.API.Stewardship.Consumers;

#pragma warning disable SA1600
public sealed class HomeownerOnboardedConsumer(IDocumentSession session)
{
    public void Handle(HomeownerOnboarded @event, CancellationToken ct)
    {
        var sc = StewardshipCase.Open(
            @event.AggregateId,
            DateOnly.FromDateTime(@event.OccurredAt.UtcDateTime),
            UserId.Create(@event.OnboardedBy));

        sc.ValidateInvariants();
        session.Events.StartStream<StewardshipCase>(sc.Id.Value, sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}
