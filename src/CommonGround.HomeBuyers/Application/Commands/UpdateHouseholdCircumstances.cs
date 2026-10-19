using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Updates a household's circumstances.</summary>
public sealed record UpdateHouseholdCircumstances(
    HouseholdId HouseholdId,
    bool RequiresAccessibility,
    decimal AnnualIncome,
    UserId UpdatedBy);

/// <summary>Handles <see cref="UpdateHouseholdCircumstances"/>.</summary>
public static class UpdateHouseholdCircumstancesHandler
{
    /// <summary>Loads the household, updates its circumstances, and persists it.</summary>
    public static async Task Handle(
        UpdateHouseholdCircumstances command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await session
            .LoadAggregateAsync<Household>(command.HouseholdId, cancellationToken)
            .ConfigureAwait(false);

        household.UpdateCircumstances(
            HouseholdCircumstances.Create(command.RequiresAccessibility, command.AnnualIncome),
            command.UpdatedBy);

        session.UpdateAggregate(household);
    }
}
