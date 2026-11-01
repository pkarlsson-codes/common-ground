using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Registers a new home on a CLT land parcel.</summary>
/// <param name="LandParcelId">The parcel this home sits on.</param>
/// <param name="UnitDesignation">Optional unit designation (e.g. "Unit B") for a parcel with several homes.</param>
/// <param name="UnitType">The physical type of the home.</param>
/// <param name="Bedrooms">Number of bedrooms.</param>
/// <param name="Bathrooms">Number of bathrooms (supports 1.5 etc.).</param>
/// <param name="IsAccessible">Whether the home is accessible to people with disabilities.</param>
/// <param name="SquareFootage">Total square footage.</param>
/// <param name="YearBuilt">Year the home was built.</param>
/// <param name="InitialAssessedValue">Initial assessed value in dollars.</param>
/// <param name="InitialAssessmentDate">The date of the initial assessment.</param>
/// <param name="AppraiserName">The name of the appraiser.</param>
/// <param name="RegisteredBy">The staff user registering the home.</param>
public sealed record RegisterHome(
    LandParcelId LandParcelId,
    string? UnitDesignation,
    HomeType UnitType,
    int Bedrooms,
    decimal Bathrooms,
    bool IsAccessible,
    decimal SquareFootage,
    int YearBuilt,
    decimal InitialAssessedValue,
    DateOnly InitialAssessmentDate,
    string? AppraiserName,
    UserId RegisteredBy);

/// <summary>Handles <see cref="RegisterHome"/> by creating and persisting a new Home.</summary>
public static class RegisterHomeHandler
{
    /// <summary>Creates a new Home and returns its ID.</summary>
    public static async Task<HomeId> Handle(
        RegisterHome command,
        IDocumentSession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var now = TimeProviderContext.Current.UtcNow;
        var today = await trustClock.ToTrustDateAsync(session.TenantId, now, cancellationToken).ConfigureAwait(false);

        var initialAssessment = UnitAssessment.Create(
            command.InitialAssessedValue,
            command.InitialAssessmentDate,
            command.AppraiserName,
            conditionNotes: null,
            command.RegisteredBy,
            now,
            today);

        var home = Home.Register(
            command.LandParcelId,
            command.UnitDesignation,
            command.UnitType,
            command.Bedrooms,
            command.Bathrooms,
            command.IsAccessible,
            command.SquareFootage,
            command.YearBuilt,
            initialAssessment,
            command.RegisteredBy);

        session.CreateAggregate<Home, HomeId>(home);

        return home.Id;
    }
}
