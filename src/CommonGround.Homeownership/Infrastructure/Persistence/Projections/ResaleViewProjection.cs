using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.Homeownership.Domain.Resales.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Homeownership.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="ResaleView"/> from Resale events.</summary>
internal sealed class ResaleViewProjection : SingleStreamProjection<ResaleView, string>
{
    public static ResaleView Create(IntentToSellRecorded e) =>
        new()
        {
            Id = e.AggregateId,
            GroundLeaseId = e.GroundLeaseId,
            LandParcelId = e.LandParcelId,
            HomeId = e.HomeId,
            HomeownerFullName = e.HomeownerFullName,
            Status = ResaleStatus.Open.DisplayName,
            NoticeDate = e.NoticeDate,
            NoticeChannel = e.Channel,
            Notes = e.Notes,
            OptionWindowEndsOn = e.OptionWindowEndsOn,
            RecordedBy = e.RecordedBy,
            RecordedAt = e.OccurredAt,
        };

    public static void Apply(ResaleView view, ResaleWithdrawn e)
    {
        view.Status = ResaleStatus.Withdrawn.DisplayName;
        view.WithdrawnDate = e.WithdrawnDate;
        view.WithdrawnReason = e.Reason;
    }
}
