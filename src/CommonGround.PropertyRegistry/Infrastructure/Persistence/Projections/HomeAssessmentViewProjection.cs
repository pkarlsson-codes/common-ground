using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.LeasableUnits.Events;
using JasperFx.Events;
using Marten.Events.Projections;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence.Projections;

internal sealed class HomeAssessmentViewProjection : EventProjection
{
    public static HomeAssessmentView Create(IEvent<LeasableUnitAssessed> e) =>
        new()
        {
            Id = e.Id,
            UnitId = e.Data.AggregateId,
            AssessedValue = e.Data.AssessedValue,
            AssessmentDate = e.Data.AssessmentDate,
            ConditionNotes = e.Data.ConditionNotes,
            AssessedBy = e.Data.AssessedBy,
            RecordedAt = e.Data.OccurredAt,
        };
}
