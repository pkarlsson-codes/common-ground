using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.PropertyRegistry.Domain.LandParcels.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Lists a land parcel for placement.</summary>
public sealed record ListParcelForPlacement(LandParcelId LandParcelId, UserId ChangedBy);

/// <summary>Handles <see cref="ListParcelForPlacement"/>.</summary>
public static class ListParcelForPlacementHandler
{
    /// <summary>Loads the parcel, lists it for placement, and persists.</summary>
    public static ParcelListedForPlacement Handle(ListParcelForPlacement command, [WriteAggregate] LandParcel parcel)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(parcel);

        return parcel.ListForPlacement(command.ChangedBy);
    }
}
