using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.Households.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Updates a household member's contact details.</summary>
public sealed record UpdateHouseholdMember(
    HouseholdId HouseholdId,
    HouseholdMemberId MemberId,
    PersonName Name,
    EmailAddress Email,
    PhoneNumber Phone,
    UserId UpdatedBy);

/// <summary>Handles <see cref="UpdateHouseholdMember"/>.</summary>
public static class UpdateHouseholdMemberHandler
{
    /// <summary>Loads the household, updates the member, and persists it.</summary>
    public static HouseholdMemberUpdated? Handle(UpdateHouseholdMember command, [WriteAggregate] Household household)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(household);

        return household.UpdateMember(command.MemberId, command.Name, command.Email, command.Phone, command.UpdatedBy);
    }
}
