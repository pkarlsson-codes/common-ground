using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.Households.Events;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Expires a mortgage prequalification if it is still current.</summary>
public sealed record ExpireMortgagePrequalification(
    HouseholdId HouseholdId,
    DateOnly OriginalExpiryDate);

/// <summary>Handles <see cref="ExpireMortgagePrequalification"/>.</summary>
public static class ExpireMortgagePrequalificationHandler
{
    /// <summary>Expires the prequalification, unless it has since been replaced or expired.</summary>
    public static MortgagePrequalificationExpired? Handle(ExpireMortgagePrequalification command, [WriteAggregate] Household household)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(household);

        return household.ExpirePrequalification(command.OriginalExpiryDate);
    }
}
