using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.Homes.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Sets a home's income limit.</summary>
/// <param name="HomeId">The home.</param>
/// <param name="PercentOfAmi">The limit as a percent of AMI, from 1 to 200, or null for no income limit.</param>
/// <param name="SetBy">The staff user setting the limit.</param>
public sealed record SetHomeIncomeLimit(
    HomeId HomeId,
    int? PercentOfAmi,
    UserId SetBy);

/// <summary>Handles <see cref="SetHomeIncomeLimit"/>.</summary>
public static class SetHomeIncomeLimitHandler
{
    /// <summary>Loads the home and records its new income limit.</summary>
    public static HomeIncomeLimitSet Handle(SetHomeIncomeLimit command, [WriteAggregate] Home home)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(home);

        return home.SetIncomeLimit(command.PercentOfAmi, command.SetBy);
    }
}
