using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.Households.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

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
    public static MortgagePrequalificationRecorded Handle(RecordMortgagePrequalification command, [WriteAggregate] Household household)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(household);

        return household.RecordMortgagePrequalification(
            command.LenderName,
            command.ApprovedAmount,
            command.ExpiryDate,
            command.RecordedBy);
    }
}
