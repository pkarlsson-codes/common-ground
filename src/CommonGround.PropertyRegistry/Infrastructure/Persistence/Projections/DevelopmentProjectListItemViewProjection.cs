using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.DevelopmentProjects.Events;
using Marten.Events.Aggregation;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence.Projections;

/// <summary>
/// Projection for <see cref="DevelopmentProjectListItemView"/>
/// </summary>
public class DevelopmentProjectListItemViewProjection : SingleStreamProjection<DevelopmentProjectListItemView, string>
{
    private static DevelopmentProjectListItemView Create(DevelopmentProjectInitiated @event)
    {
        return new DevelopmentProjectListItemView
        {
            Id = @event.AggregateId,
            LandParcelId = @event.LandParcelId,
            HomeIds = @event.HomeIds,
            Name = @event.Name,
            Description = @event.Description,
            Status = "Initiated",
            ProjectType = @event.ProjectType == 2 ? "Rehabilitation" : "NewConstruction",
            CreatedAt = @event.OccurredAt,
        };
    }
}
