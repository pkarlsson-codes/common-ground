using CommonGround.Homeownership.Contracts.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.Stewardship.Domain.StewardshipCases;
using Marten;

namespace CommonGround.Stewardship.Application.EventHandlers;

/// <summary>Opens a StewardshipCase when a homeowner is onboarded in the Homeownership BC.</summary>
public sealed class HomeownerOnboardedHandler(IDocumentSession session)
{
    /// <summary>Handles the integration event.</summary>
    public void Handle(HomeownerOnboarded @event, CancellationToken ct)
    {
        var sc = StewardshipCase.Open(
            @event.GroundLeaseId,
            DateOnly.FromDateTime(@event.OccurredAt.UtcDateTime),
            UserId.Create(@event.OnboardedBy));

        sc.ValidateInvariants();
        session.Events.StartStream<StewardshipCase>(sc.Id.Value, sc.UncommittedEvents.ToArray());
        sc.UncommittedEvents.MarkAsCommitted();
    }
}
