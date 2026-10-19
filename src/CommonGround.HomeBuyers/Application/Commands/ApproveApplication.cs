using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Approves eligibility for a homeowner application.</summary>
public sealed record ApproveApplication(
    HomeownerApplicationId ApplicationId,
    DateOnly ApprovedDate,
    string? Notes,
    UserId RecordedBy);

/// <summary>Handles <see cref="ApproveApplication"/>.</summary>
public static class ApproveApplicationHandler
{
    /// <summary>Loads the application, approves eligibility, and persists it.</summary>
    public static async Task Handle(
        ApproveApplication command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var application = await session
            .LoadAggregateAsync<HomeownerApplication>(command.ApplicationId, cancellationToken)
            .ConfigureAwait(false);

        application.Approve(command.ApprovedDate, command.Notes, command.RecordedBy);

        session.UpdateAggregate(application);
    }
}