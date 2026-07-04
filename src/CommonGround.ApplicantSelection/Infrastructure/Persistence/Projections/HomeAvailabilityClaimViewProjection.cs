using CommonGround.ApplicantSelection.Application.Errors;
using CommonGround.ApplicantSelection.Application.ReadModels;
using CommonGround.ApplicantSelection.Domain.HomeownerApplications.Events;
using JasperFx.Events;
using Marten.Events.Projections;

namespace CommonGround.ApplicantSelection.Infrastructure.Persistence.Projections;

/// <summary>
/// Guards against two applications simultaneously claiming the same home. Grouped by
/// <see cref="ApplicantSelectedForHome.HomeId"/> rather than the source stream's ApplicationId,
/// and registered Inline so the guard runs in the same transaction as the selecting command.
/// </summary>
internal sealed class HomeAvailabilityClaimViewProjection : MultiStreamProjection<HomeAvailabilityClaimView, string>
{
    public HomeAvailabilityClaimViewProjection()
    {
        Identity<ApplicantSelectedForHome>(e => e.HomeId);
    }

    public static HomeAvailabilityClaimView Create(IEvent<ApplicantSelectedForHome> e) =>
        new()
        {
            Id = e.Data.HomeId,
            ApplicationId = e.Data.AggregateId,
        };

    public static void Apply(HomeAvailabilityClaimView view, ApplicantSelectedForHome e)
    {
        if (view.ApplicationId != e.AggregateId)
        {
            throw BusinessRuleViolation.HomeAvailabilityClaim.AlreadyClaimed(e.HomeId);
        }
    }
}
