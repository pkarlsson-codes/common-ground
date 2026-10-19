using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Records a mortgage prequalification for a homeowner application.</summary>
public sealed record RecordMortgagePrequalification(
    HouseholdId HouseholdId,
    string LenderName,
    decimal ApprovedAmount,
    DateOnly ExpiryDate,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordMortgagePrequalification"/>.</summary>
public static class RecordMortgagePrequalificationHandler
{
    /// <summary>Loads the application, records the prequalification, and persists it.</summary>
    public static async Task Handle(
        RecordMortgagePrequalification command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await session
            .LoadAggregateAsync<Household>(command.HouseholdId, cancellationToken)
            .ConfigureAwait(false);

        household.RecordMortgagePrequalification(
            command.LenderName,
            command.ApprovedAmount,
            command.ExpiryDate,
            command.RecordedBy);

        session.UpdateAggregate(household);
    }
}
