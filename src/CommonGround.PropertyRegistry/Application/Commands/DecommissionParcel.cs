using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.PropertyRegistry.Domain.LandParcels.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Decommissions a parcel.</summary>
public sealed record DecommissionParcel(LandParcelId LandParcelId, UserId ChangedBy);

/// <summary>Handles <see cref="DecommissionParcel"/>.</summary>
public static class DecommissionParcelHandler
{
    /// <summary>Loads the parcel, decommissions it, and persists.</summary>
    public static ParcelDecommissioned? Handle(DecommissionParcel command, [WriteAggregate] LandParcel parcel)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(parcel);

        return parcel.Decommission(command.ChangedBy);
    }
}
