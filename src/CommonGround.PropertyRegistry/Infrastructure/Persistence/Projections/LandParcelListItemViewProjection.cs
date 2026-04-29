using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.LandParcels.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="LandParcelListItemView"/> from LandParcel events.</summary>
internal sealed class LandParcelListItemViewProjection : SingleStreamProjection<LandParcelListItemView, string>
{
    /// <summary>Creates the initial list item from a LandParcelRegistered event.</summary>
    public static LandParcelListItemView Create(IEvent<LandParcelRegistered> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            LegalParcelNumber = e.Data.LegalParcelNumber,
            Address = e.Data.Address,
            AreaAcres = e.Data.AreaAcres,
            RegisteredAt = e.Data.OccurredAt,
        };

    /// <summary>Marks the view as decommissioned.</summary>
    public static void Apply(LandParcelListItemView view, ParcelDecommissioned _)
    {
        view.IsDecommissioned = true;
    }

    /// <summary>Clears the decommissioned flag on recommission.</summary>
    public static void Apply(LandParcelListItemView view, ParcelRecommissioned _)
    {
        view.IsDecommissioned = false;
    }
}
