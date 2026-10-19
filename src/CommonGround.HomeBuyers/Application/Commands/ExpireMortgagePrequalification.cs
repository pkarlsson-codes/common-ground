using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Expires a mortgage prequalification if it is still current.</summary>
public sealed record ExpireMortgagePrequalification(
    HouseholdId ApplicationId,
    DateOnly OriginalExpiryDate);

/// <summary>Handles <see cref="ExpireMortgagePrequalification"/>.</summary>
public static class ExpireMortgagePrequalificationHandler
{
    /// <summary>Loads the household, expires the prequalification when applicable, and persists it.</summary>
    public static async Task Handle(
        ExpireMortgagePrequalification command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
    
        var household = await session
            .LoadAggregateAsync<Household>(command.ApplicationId, cancellationToken)
            .ConfigureAwait(false);

        var changed = household.ExpirePrequalification(command.OriginalExpiryDate);
        if (changed)
            session.UpdateAggregate(household);
    }
}
