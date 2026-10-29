using CommonGround.Homeownership.Contracts.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using CommonGround.Stewardship.Domain.StewardshipCases;
using Marten;

namespace CommonGround.Stewardship.Application.EventHandlers;

/// <summary>Opens a StewardshipCase when a ground lease is created in the Homeownership BC.</summary>
public sealed class GroundLeaseCreatedHandler(IDocumentSession session, ITrustClock trustClock)
{
    /// <summary>Handles the integration event.</summary>
    public async Task Handle(GroundLeaseCreated @event, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var openedOn = await trustClock.ToTrustDateAsync(session.TenantId, @event.OccurredAt, ct).ConfigureAwait(false);
        var sc = StewardshipCase.Open(
            @event.GroundLeaseId,
            openedOn,
            UserId.Create(@event.OnboardedBy));

        session.CreateAggregate<StewardshipCase, StewardshipCaseId>(sc);
    }
}
