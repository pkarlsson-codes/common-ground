using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

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
    public static ApplicationApproved Handle(ApproveApplication command, [WriteAggregate(nameof(ApproveApplication.ApplicationId))] HomeownerApplication application)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(application);

        return application.Approve(command.ApprovedDate, command.Notes, command.RecordedBy);
    }
}