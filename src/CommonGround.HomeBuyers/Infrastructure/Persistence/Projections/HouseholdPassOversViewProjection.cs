using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using Marten.Events.Projections;

namespace CommonGround.HomeBuyers.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="HouseholdPassOversView"/> from pass-overs across a household's applications.</summary>
internal sealed class HouseholdPassOversViewProjection : MultiStreamProjection<HouseholdPassOversView, string>
{
    /// <summary>Routes each pass-over to its household.</summary>
    public HouseholdPassOversViewProjection()
    {
        Identity<ApplicationPassedOver>(e => e.HouseholdId);
    }

    public static void Apply(HouseholdPassOversView view, ApplicationPassedOver e)
    {
        view.Id = e.HouseholdId;
        view.PassOvers.Add(new HouseholdPassOverEntry
        {
            ApplicationId = e.AggregateId,
            SelectionId = e.SelectionId,
            HomeId = e.HomeId,
            PassedOverAt = e.OccurredAt,
        });
    }
}
