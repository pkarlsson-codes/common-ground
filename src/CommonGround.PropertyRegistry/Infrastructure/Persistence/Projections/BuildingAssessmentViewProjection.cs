using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Buildings.Events;
using JasperFx.Events;
using Marten.Events.Projections;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence.Projections;

internal sealed class BuildingAssessmentViewProjection : EventProjection
{
    public static BuildingAssessmentView Create(IEvent<BuildingAssessed> e) =>
        new()
        {
            Id = e.Id,
            BuildingId = e.Data.AggregateId,
            AssessedValue = e.Data.AssessedValue,
            AssessmentDate = e.Data.AssessmentDate,
            ConditionNotes = e.Data.ConditionNotes,
            AssessedBy = e.Data.AssessedBy,
            RecordedAt = e.Data.OccurredAt,
        };
}
