using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Commands;

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

        var household = await session
            .LoadAggregateAsync<Household>(application.HouseholdId, cancellationToken)
            .ConfigureAwait(false);

        var homeLandParcel = await session
            .LoadAsync<HomeLandParcelView>(command.HomeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.HomeId);

        application.SelectForHome(
            household,
            command.HomeId,
            homeLandParcel.LandParcelId,
            command.GroundLeaseTemplateId,
            command.PurchasePrice,
            command.SelectedDate,
            command.SelectedBy);

        session.UpdateAggregate(application);
    }
}
