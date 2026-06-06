using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.ApplicantSelection.Application.Commands;

/// <summary>Approves eligibility for a homeowner application.</summary>
public sealed record ApproveEligibility(
    HomeownerApplicationId ApplicationId,
    DateOnly ApprovedDate,
    string? Notes,
    UserId RecordedBy);

/// <summary>Handles <see cref="ApproveEligibility"/>.</summary>
public static class ApproveEligibilityHandler
{
    /// <summary>Loads the application, approves eligibility, and persists it.</summary>
    public static async Task Handle(
        ApproveEligibility command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var application = await session
            .LoadAggregateAsync<HomeownerApplication>(command.ApplicationId, cancellationToken)
            .ConfigureAwait(false);

        application.ApproveEligibility(command.ApprovedDate, command.Notes, command.RecordedBy);

        session.UpdateAggregate(application);
    }
}