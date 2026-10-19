using CommonGround.HomeBuyers.Application.Errors;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using JasperFx.Events;
using Marten.Events.Projections;

namespace CommonGround.HomeBuyers.Infrastructure.Persistence.Projections;

/// <summary>
/// Guards against two applications simultaneously claiming the same home. Grouped by
/// <see cref="ApplicationSelectedForHome.HomeId"/> rather than the source stream's ApplicationId,
/// and registered Inline so the guard runs in the same transaction as the selecting command.
/// </summary>
internal sealed class HomeAvailabilityClaimViewProjection : MultiStreamProjection<HomeAvailabilityClaimView, string>
{
    public HomeAvailabilityClaimViewProjection()
    {
        Identity<ApplicationSelectedForHome>(e => e.HomeId);
    }

    public static HomeAvailabilityClaimView Create(IEvent<ApplicationSelectedForHome> e) =>
        new()
        {
            Id = e.Data.HomeId,
            ApplicationId = e.Data.AggregateId,
        };

    public static void Apply(HomeAvailabilityClaimView view, ApplicationSelectedForHome e)
    {
        if (view.ApplicationId != e.AggregateId)
        {
            throw BusinessRuleViolation.HomeAvailabilityClaim.AlreadyClaimed(e.HomeId);
        }
    }
}
