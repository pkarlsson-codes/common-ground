using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Updates a household member's contact details.</summary>
public sealed record UpdateHouseholdMember(
    HouseholdId HouseholdId,
    HouseholdMemberId MemberId,
    string Name,
    string Email,
    string Phone,
    UserId UpdatedBy);

/// <summary>Handles <see cref="UpdateHouseholdMember"/>.</summary>
public static class UpdateHouseholdMemberHandler
{
    /// <summary>Loads the household, updates the member, and persists it.</summary>
    public static async Task Handle(
        UpdateHouseholdMember command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await session
            .LoadAggregateAsync<Household>(command.HouseholdId, cancellationToken)
            .ConfigureAwait(false);

        household.UpdateMember(command.MemberId, command.Name, command.Email, command.Phone, command.UpdatedBy);

        session.UpdateAggregate(household);
    }
}
