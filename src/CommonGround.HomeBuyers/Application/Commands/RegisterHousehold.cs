using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Registers a household.</summary>
public sealed record RegisterHousehold(
    HouseholdId HouseholdId,
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
    /// <summary>Starts the household's stream.</summary>
    public static IStartStream Handle(RegisterHousehold command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var members = command.Members
            .Select(m => HouseholdMember.Create(m.Name, m.Email, m.Phone, m.IsPrimary))
            .ToList();
        var circumstances = HouseholdCircumstances.Create(command.RequiresAccessibility, command.AnnualIncome);
        var registered = Household.Register(command.HouseholdId, members, circumstances, command.RegisteredBy);

        return MartenOps.StartStream<Household>(command.HouseholdId.Value, registered);
    }
}
