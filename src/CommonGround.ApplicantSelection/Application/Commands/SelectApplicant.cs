using CommonGround.ApplicantSelection.Application.ReadModels;
using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.ApplicantSelection.Application.Commands;

/// <summary>Selects an applicant for a specific home. LandParcelId is resolved server-side from the home.</summary>
public sealed record SelectApplicant(
    HomeownerApplicationId ApplicationId,
    string HomeId,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    DateOnly SelectedDate,
    UserId SelectedBy);

/// <summary>Handles <see cref="SelectApplicant"/>.</summary>
public static class SelectApplicantHandler
{
    /// <summary>Loads the application, resolves the home's land parcel, selects the applicant, and persists it.</summary>
    public static async Task Handle(
        SelectApplicant command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var application = await session
            .LoadAggregateAsync<HomeownerApplication>(command.ApplicationId, cancellationToken)
            .ConfigureAwait(false);

        var homeLandParcel = await session
            .LoadAsync<HomeLandParcelView>(command.HomeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.HomeId);

        application.SelectForHome(
            command.HomeId,
            homeLandParcel.LandParcelId,
            command.GroundLeaseTemplateId,
            command.PurchasePrice,
            command.SelectedDate,
            command.SelectedBy);

        session.UpdateAggregate(application);
    }
}
