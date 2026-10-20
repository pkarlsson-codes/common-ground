using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Homes.Events;
using JasperFx.Events;
using Marten.Events.Projections;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence.Projections;

internal sealed class HomeAssessmentViewProjection : EventProjection
{
    public static HomeAssessmentView Create(IEvent<HomeAssessed> e) =>
        new()
        {
            Id = e.Id,
            UnitId = e.Data.AggregateId,
            AssessedValue = e.Data.AssessedValue,
            AssessmentDate = e.Data.AssessmentDate,
            ConditionNotes = e.Data.ConditionNotes,
            AppraiserName = e.Data.AppraiserName,
            RecordedBy = e.Data.RecordedById,
            RecordedAt = e.Data.OccurredAt,
        };

    public static HomeAssessmentView Create(IEvent<HomeRegistered> e) =>
        new()
        {
            Id = e.Id,
            UnitId = e.Data.AggregateId,
            AssessedValue = e.Data.InitialAssessedValue,
            AssessmentDate = e.Data.InitiallyAssessedAt,
            AppraiserName = e.Data.InitialAppraiserName,
            RecordedBy = e.Data.RegisteredBy,
            RecordedAt = e.Data.OccurredAt,
        };
}
