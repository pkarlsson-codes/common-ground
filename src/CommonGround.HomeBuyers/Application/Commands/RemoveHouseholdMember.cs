using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.Households.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Removes a non-primary member from a household.</summary>
public sealed record RemoveHouseholdMember(
    HouseholdId HouseholdId,
    HouseholdMemberId MemberId,
    UserId RemovedBy);

/// <summary>Handles <see cref="RemoveHouseholdMember"/>.</summary>
public static class RemoveHouseholdMemberHandler
{
    /// <summary>Loads the household, removes the member, and persists it.</summary>
    public static HouseholdMemberRemoved Handle(RemoveHouseholdMember command, [WriteAggregate] Household household)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(household);

        return household.RemoveMember(command.MemberId, command.RemovedBy);
    }
}
