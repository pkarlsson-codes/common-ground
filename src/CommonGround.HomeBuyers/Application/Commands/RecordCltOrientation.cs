using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.Households.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using Marten;
using Wolverine.Marten;

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
    public static async Task<CltOrientationRecorded?> Handle(
        RecordCltOrientation command,
        [WriteAggregate] Household household,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(household);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);
        return household.RecordCltOrientation(command.CompletedDate, today, command.RecordedBy);
    }
}