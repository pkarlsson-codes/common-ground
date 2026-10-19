using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Adds a non-primary member to a household.</summary>
public sealed record AddHouseholdMember(
    HouseholdId HouseholdId,
    string Name,
    string Email,
    string Phone,
    UserId AddedBy);

/// <summary>Handles <see cref="AddHouseholdMember"/>.</summary>
public static class AddHouseholdMemberHandler
{
    /// <summary>Loads the household, adds the member, persists it, and returns the new member's ID.</summary>
    public static async Task<HouseholdMemberId> Handle(
        AddHouseholdMember command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await session
            .LoadAggregateAsync<Household>(command.HouseholdId, cancellationToken)
            .ConfigureAwait(false);

        var memberId = household.AddMember(command.Name, command.Email, command.Phone, command.AddedBy);

        session.UpdateAggregate(household);
        return memberId;
    }
}
