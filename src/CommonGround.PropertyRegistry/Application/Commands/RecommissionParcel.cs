using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.PropertyRegistry.Domain.LandParcels.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Recommissions a decommissioned parcel.</summary>
public sealed record RecommissionParcel(LandParcelId LandParcelId, UserId ChangedBy);

/// <summary>Handles <see cref="RecommissionParcel"/>.</summary>
public static class RecommissionParcelHandler
{
    /// <summary>Loads the parcel, recommissions it, and persists.</summary>
    public static ParcelRecommissioned? Handle(RecommissionParcel command, [WriteAggregate] LandParcel parcel)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(parcel);

        return parcel.Recommission(command.ChangedBy);
    }
}
