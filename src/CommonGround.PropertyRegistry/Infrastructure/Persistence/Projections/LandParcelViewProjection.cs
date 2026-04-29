using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.LandParcels.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="LandParcelView"/> from LandParcel events.</summary>
internal sealed class LandParcelViewProjection : SingleStreamProjection<LandParcelView, string>
{
    /// <summary>Creates the initial view from a LandParcelRegistered event.</summary>
    public static LandParcelView Create(IEvent<LandParcelRegistered> e) =>
        new()
        {
            Id = e.Data.AggregateId,
            LegalParcelNumber = e.Data.LegalParcelNumber,
            Address = e.Data.Address,
            AreaAcres = e.Data.AreaAcres,
            AcquisitionDate = e.Data.AcquisitionDate,
            AcquisitionMethod = e.Data.AcquisitionMethod,
            CovenantTerms = e.Data.CovenantTerms,
            RegisteredAt = e.Data.OccurredAt,
            RegisteredBy = e.Data.RegisteredBy,
        };

    /// <summary>Marks the view as decommissioned.</summary>
    public static void Apply(LandParcelView view, ParcelDecommissioned _)
    {
        view.IsDecommissioned = true;
    }

    /// <summary>Clears the decommissioned flag on recommission.</summary>
    public static void Apply(LandParcelView view, ParcelRecommissioned _)
    {
        view.IsDecommissioned = false;
    }
}
