using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Decommissions a parcel.</summary>
public sealed record DecommissionParcel(LandParcelId LandParcelId, UserId ChangedBy);

/// <summary>Handles <see cref="DecommissionParcel"/>.</summary>
public static class DecommissionParcelHandler
{
    /// <summary>Loads the parcel, decommissions it, and persists.</summary>
    public static async Task Handle(
        DecommissionParcel command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events.FetchForWriting<LandParcel>(command.LandParcelId.Value, cancellationToken).ConfigureAwait(false);
        var parcel = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(LandParcel), command.LandParcelId.Value);

        parcel.Decommission(command.ChangedBy);

        parcel.ValidateInvariants();
        stream.AppendMany(parcel.UncommittedEvents.ToArray());
        parcel.UncommittedEvents.MarkAsCommitted();
    }
}
