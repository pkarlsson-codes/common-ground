using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
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

        var parcel = await session
            .LoadAggregateAsync<LandParcel>(command.LandParcelId, cancellationToken)
            .ConfigureAwait(false);

        parcel.ListForPlacement(command.ChangedBy);

        session.UpdateAggregate(parcel);
    }
}
