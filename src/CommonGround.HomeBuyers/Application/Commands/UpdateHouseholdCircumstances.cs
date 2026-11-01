using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.Households.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

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
    public static HouseholdCircumstancesUpdated? Handle(UpdateHouseholdCircumstances command, [WriteAggregate] Household household)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(household);

        return household.UpdateCircumstances(
            HouseholdCircumstances.Create(command.RequiresAccessibility, command.AnnualIncome),
            command.UpdatedBy);
    }
}
