using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Records CLT orientation completion for a homeowner application.</summary>
public sealed record RecordCltOrientation(
    HouseholdId HouseholdId,
    DateOnly CompletedDate,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordCltOrientation"/>.</summary>
public static class RecordCltOrientationHandler
{
    /// <summary>Loads the application, records orientation completion, and persists it.</summary>
    public static async Task Handle(
        RecordCltOrientation command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await session
            .LoadAggregateAsync<Household>(command.HouseholdId, cancellationToken)
            .ConfigureAwait(false);

        household.RecordCltOrientation(command.CompletedDate, command.RecordedBy);

        session.UpdateAggregate(household);
    }
}