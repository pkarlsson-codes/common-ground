using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.ApplicantSelection.Application.Commands;

/// <summary>Selects an applicant for a specific home.</summary>
public sealed record SelectApplicant(
    HomeownerApplicationId ApplicationId,
    string HomeId,
    string LandParcelId,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    DateOnly SelectedDate,
    UserId SelectedBy);

/// <summary>Handles <see cref="SelectApplicant"/>.</summary>
public static class SelectApplicantHandler
{
    /// <summary>Loads the application, selects the applicant, and persists it.</summary>
    public static async Task Handle(
        SelectApplicant command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var application = await session
            .LoadAggregateAsync<HomeownerApplication>(command.ApplicationId, cancellationToken)
            .ConfigureAwait(false);

        application.SelectForHome(
            command.HomeId,
            command.LandParcelId,
            command.GroundLeaseTemplateId,
            command.PurchasePrice,
            command.SelectedDate,
            command.SelectedBy);

        session.UpdateAggregate(application);
    }
}
