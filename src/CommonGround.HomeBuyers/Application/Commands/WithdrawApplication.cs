using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Withdraws a homeowner application.</summary>
public sealed record WithdrawApplication(
    HomeownerApplicationId ApplicationId,
    DateOnly WithdrawnDate,
    string? Reason,
    UserId RecordedById)
{
}

/// <summary>Handles <see cref="WithdrawApplication"/>.</summary>
public static class WithdrawApplicationHandler
{
    /// <summary>Loads the application, withdraws it, and persists it.</summary>
    public static ApplicationWithdrawn Handle(WithdrawApplication command, [WriteAggregate(nameof(WithdrawApplication.ApplicationId))] HomeownerApplication application)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(application);

        return application.Withdraw(command.WithdrawnDate, command.Reason, command.RecordedById);
    }
}