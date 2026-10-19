using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Registers a household.</summary>
public sealed record RegisterHousehold(
    IReadOnlyList<RegisterHouseholdMember> Members,
    bool RequiresAccessibility,
    decimal AnnualIncome,
    UserId RegisteredBy);

/// <summary>A member to register as part of <see cref="RegisterHousehold"/>.</summary>
public sealed record RegisterHouseholdMember(
    string Name,
    string Email,
    string Phone,
    bool IsPrimary);

/// <summary>Handles <see cref="RegisterHousehold"/>.</summary>
public static class RegisterHouseholdHandler
{
    /// <summary>Registers the household and persists it.</summary>
    public static HouseholdId Handle(
        RegisterHousehold command,
        IDocumentSession session)
    {
        ArgumentNullException.ThrowIfNull(command);

        var members = command.Members
            .Select(m => HouseholdMember.Create(m.Name, m.Email, m.Phone, m.IsPrimary))
            .ToList();
        var circumstances = HouseholdCircumstances.Create(command.RequiresAccessibility, command.AnnualIncome);
        var household = Household.Register(members, circumstances, command.RegisteredBy);
        session.CreateAggregate<Household, HouseholdId>(household);
        return household.Id;
    }
}
