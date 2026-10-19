using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

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
    public static async Task Handle(
        RecordHousingCounseling command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await session
            .LoadAggregateAsync<Household>(command.HouseholdId, cancellationToken)
            .ConfigureAwait(false);

        household.RecordHousingCounseling(
            command.CertificationNumber,
            command.CompletedDate,
            command.CounselorOrganization,
            command.RecordedBy);

        session.UpdateAggregate(household);
    }
}