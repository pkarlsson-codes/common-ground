using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

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
    public static async Task Handle(
        RemoveHouseholdMember command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await session
            .LoadAggregateAsync<Household>(command.HouseholdId, cancellationToken)
            .ConfigureAwait(false);

        household.RemoveMember(command.MemberId, command.RemovedBy);

        session.UpdateAggregate(household);
    }
}
