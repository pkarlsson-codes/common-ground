using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Submits a new homeowner application.</summary>
public sealed record SubmitApplication(
    HouseholdId HouseholdId,
    UserId SubmittedBy);

/// <summary>Handles <see cref="SubmitApplication"/>.</summary>
public static class SubmitApplicationHandler
{
    private static readonly string[] OpenPhases =
        [.. ApplicationPhase.GetAll().Where(p => !p.IsTerminal).Select(p => p.DisplayName)];

    /// <summary>Creates a new homeowner application and returns its ID.</summary>
    public static async Task<HomeownerApplicationId> Handle(
        SubmitApplication command,
        IDocumentSession session)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await session
            .LoadAggregateAsync<Household>(command.HouseholdId)
            .ConfigureAwait(false);

        var openApplicationId = await session.Query<HomeownerApplicationListItemView>()
            .Where(v => v.HouseholdId == command.HouseholdId.Value && OpenPhases.Contains(v.Phase))
            .Select(v => v.Id)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        var application = HomeownerApplication.Submit(
            command.HouseholdId,
            household.Declare(),
            openApplicationId is null ? null : HomeownerApplicationId.Create(openApplicationId),
            command.SubmittedBy);

        session.CreateAggregate<HomeownerApplication, HomeownerApplicationId>(application);

        return application.Id;
    }
}
