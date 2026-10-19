using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

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
    public static async Task Handle(
        WithdrawApplication command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var application = await session
            .LoadAggregateAsync<HomeownerApplication>(command.ApplicationId, cancellationToken)
            .ConfigureAwait(false);

        application.Withdraw(command.WithdrawnDate, command.Reason, command.RecordedById);

        session.UpdateAggregate(application);
    }
}