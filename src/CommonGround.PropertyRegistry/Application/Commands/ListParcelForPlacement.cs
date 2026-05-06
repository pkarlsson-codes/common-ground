using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Lists a land parcel for placement.</summary>
public sealed record ListParcelForPlacement(LandParcelId LandParcelId, UserId ChangedBy);

/// <summary>Handles <see cref="ListParcelForPlacement"/>.</summary>
public static class ListParcelForPlacementHandler
{
    /// <summary>Loads the parcel, lists it for placement, and persists.</summary>
    public static async Task Handle(
        ListParcelForPlacement command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events.FetchForWriting<LandParcel>(command.LandParcelId.Value, cancellationToken).ConfigureAwait(false);
        var parcel = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(LandParcel), command.LandParcelId.Value);

        parcel.ListForPlacement(command.ChangedBy);

        parcel.ValidateInvariants();
        stream.AppendMany(parcel.UncommittedEvents.ToArray());
        parcel.UncommittedEvents.MarkAsCommitted();
    }
}
