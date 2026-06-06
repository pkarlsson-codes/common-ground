using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.ApplicantSelection.Application.Commands;

/// <summary>Denies eligibility for a homeowner application.</summary>
public sealed record DenyEligibility(
    HomeownerApplicationId ApplicationId,
    string Reason,
    DateOnly DeniedDate,
    UserId RecordedBy);

/// <summary>Handles <see cref="DenyEligibility"/>.</summary>
public static class DenyEligibilityHandler
{
    /// <summary>Loads the application, denies eligibility, and persists it.</summary>
    public static async Task Handle(
        DenyEligibility command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var application = await session
            .LoadAggregateAsync<HomeownerApplication>(command.ApplicationId, cancellationToken)
            .ConfigureAwait(false);

        application.DenyEligibility(command.Reason, command.DeniedDate, command.RecordedBy);

        session.UpdateAggregate(application);
    }
}