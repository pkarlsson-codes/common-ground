using CommonGround.Homeownership.Contracts.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using CommonGround.Stewardship.Domain.StewardshipCases;
using Marten;

namespace CommonGround.Stewardship.Application.EventHandlers;

/// <summary>Opens a StewardshipCase when a ground lease is created in the Homeownership BC.</summary>
public sealed class GroundLeaseCreatedHandler(IDocumentSession session)
{
    /// <summary>Handles the integration event.</summary>
    public void Handle(GroundLeaseCreated @event, CancellationToken ct)
    {
        var sc = StewardshipCase.Open(
            @event.GroundLeaseId,
            DateOnly.FromDateTime(@event.OccurredAt.UtcDateTime),
            UserId.Create(@event.OnboardedBy));

        session.CreateAggregate<StewardshipCase, StewardshipCaseId>(sc);
    }
}
