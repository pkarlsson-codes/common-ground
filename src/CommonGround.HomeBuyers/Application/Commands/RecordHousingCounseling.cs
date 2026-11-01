using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.Households.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

/// <summary>Records housing counseling completion for a homeowner application.</summary>
public sealed record RecordHousingCounseling(
    HouseholdId HouseholdId,
    string CertificationNumber,
    DateOnly CompletedDate,
    string CounselorOrganization,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordHousingCounseling"/>.</summary>
public static class RecordHousingCounselingHandler
{
    /// <summary>Loads the application, records counseling completion, and persists it.</summary>
    public static HousingCounselingRecorded? Handle(RecordHousingCounseling command, [WriteAggregate] Household household)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(household);

        return household.RecordHousingCounseling(
            command.CertificationNumber,
            command.CompletedDate,
            command.CounselorOrganization,
            command.RecordedBy);
    }
}