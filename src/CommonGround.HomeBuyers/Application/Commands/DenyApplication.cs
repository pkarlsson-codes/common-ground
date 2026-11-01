using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Denies a homeowner application.</summary>
public sealed record DenyApplication(
    HomeownerApplicationId ApplicationId,
    string Reason,
    DateOnly DeniedDate,
    UserId RecordedBy);

/// <summary>Handles <see cref="DenyApplication"/>.</summary>
public static class DenyApplicationHandler
{
    /// <summary>Loads the application, denies it, and persists it.</summary>
    public static ApplicationDenied Handle(DenyApplication command, [WriteAggregate(nameof(DenyApplication.ApplicationId))] HomeownerApplication application)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(application);

        return application.DenyApplication(command.Reason, command.DeniedDate, command.RecordedBy);
    }
}