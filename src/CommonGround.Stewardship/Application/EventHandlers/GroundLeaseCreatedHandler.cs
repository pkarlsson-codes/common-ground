using CommonGround.Homeownership.Contracts.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.Stewardship.Domain.StewardshipCases;
using Wolverine;
using Wolverine.Marten;

namespace CommonGround.Stewardship.Application.EventHandlers;

/// <summary>Opens a StewardshipCase when a ground lease is created in the Homeownership BC.</summary>
public static class GroundLeaseCreatedHandler
{
    /// <summary>Resolves the date the lease was created in the trust's time zone.</summary>
    public static async Task<DateOnly> LoadAsync(
        GroundLeaseCreated @event,
        ITrustClock trustClock,
        IMessageContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(trustClock);
        ArgumentNullException.ThrowIfNull(context);

        return await trustClock.ToTrustDateAsync(context.TenantId!, @event.OccurredAt, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Starts the lease's stewardship case.</summary>
    public static IStartStream Handle(GroundLeaseCreated @event, DateOnly openedOn)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var id = StewardshipCaseId.Generate();
        var opened = StewardshipCase.Open(id, @event.GroundLeaseId, openedOn, UserId.Create(@event.OnboardedBy));

        return MartenOps.StartStream<StewardshipCase>(id.Value, opened);
    }
}
