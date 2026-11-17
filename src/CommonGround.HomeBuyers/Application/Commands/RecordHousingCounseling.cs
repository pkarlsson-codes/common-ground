using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.Households.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using Marten;
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
    public static async Task<HousingCounselingRecorded?> Handle(
        RecordHousingCounseling command,
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
        return household.RecordHousingCounseling(
            command.CertificationNumber,
            command.CompletedDate,
            command.CounselorOrganization,
            today,
            command.RecordedBy);
    }
}