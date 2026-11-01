using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.Households.Events;
using CommonGround.SharedKernel.Domain;
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
    public static CltOrientationRecorded? Handle(RecordCltOrientation command, [WriteAggregate] Household household)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(household);

        return household.RecordCltOrientation(command.CompletedDate, command.RecordedBy);
    }
}