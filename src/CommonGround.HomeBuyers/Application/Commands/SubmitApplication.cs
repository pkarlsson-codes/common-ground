using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using Marten;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Submits a new homeowner application.</summary>
/// <param name="ApplicationId">The new application's ID, chosen by the caller.</param>
/// <param name="HouseholdId">The household applying.</param>
/// <param name="SubmittedBy">The staff user submitting the application.</param>
public sealed record SubmitApplication(
    HomeownerApplicationId ApplicationId,
    HouseholdId HouseholdId,
    UserId SubmittedBy);

/// <summary>Handles <see cref="SubmitApplication"/>.</summary>
public static class SubmitApplicationHandler
{
    private static readonly string[] OpenPhases =
        [.. ApplicationPhase.GetAll().Where(p => !p.IsTerminal).Select(p => p.DisplayName)];

    /// <summary>Finds the household's application that is still open, if any.</summary>
    public static async Task<HomeownerApplicationListItemView?> LoadAsync(
        SubmitApplication command,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(session);

        return await session.Query<HomeownerApplicationListItemView>()
            .Where(v => v.HouseholdId == command.HouseholdId.Value && OpenPhases.Contains(v.Phase))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Starts the application with the household's declaration, unless it already has one open.</summary>
    public static IStartStream Handle(
        SubmitApplication command,
        [ReadAggregate] Household household,
        HomeownerApplicationListItemView? openApplication)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(household);

        var submitted = HomeownerApplication.Submit(
            command.ApplicationId,
            command.HouseholdId,
            household.Declare(),
            EntityIds.CreateOrNull<HomeownerApplicationId>(openApplication?.Id),
            command.SubmittedBy);

        return MartenOps.StartStream<HomeownerApplication>(command.ApplicationId.Value, submitted);
    }
}
