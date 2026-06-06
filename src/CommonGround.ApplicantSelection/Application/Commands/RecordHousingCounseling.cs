using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.ApplicantSelection.Application.Commands;

/// <summary>Records housing counseling completion for a homeowner application.</summary>
public sealed record RecordHousingCounseling(
    HomeownerApplicationId ApplicationId,
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

        var application = await session
            .LoadAggregateAsync<HomeownerApplication>(command.ApplicationId, cancellationToken)
            .ConfigureAwait(false);

        application.RecordHousingCounseling(
            command.CertificationNumber,
            command.CompletedDate,
            command.CounselorOrganization,
            command.RecordedBy);

        session.UpdateAggregate(application);
    }
}