using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.Households.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Adds a non-primary member to a household.</summary>
public sealed record AddHouseholdMember(
    HouseholdId HouseholdId,
    HouseholdMemberId MemberId,
    PersonName Name,
    EmailAddress Email,
    PhoneNumber Phone,
    UserId AddedBy);

/// <summary>Handles <see cref="AddHouseholdMember"/>.</summary>
public static class AddHouseholdMemberHandler
{
    /// <summary>Adds the member to the household.</summary>
    public static HouseholdMemberAdded Handle(AddHouseholdMember command, [WriteAggregate] Household household)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(household);

        return household.AddMember(command.MemberId, command.Name, command.Email, command.Phone, command.AddedBy);
    }
}
