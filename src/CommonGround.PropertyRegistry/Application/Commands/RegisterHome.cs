using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using Wolverine;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Registers a new home on a CLT land parcel.</summary>
/// <param name="HomeId">The new home's ID, chosen by the caller.</param>
/// <param name="LandParcelId">The parcel this home sits on.</param>
/// <param name="UnitDesignation">Optional unit designation (e.g. "Unit B") for a parcel with several homes.</param>
/// <param name="UnitType">The physical type of the home.</param>
/// <param name="Bedrooms">Number of bedrooms.</param>
/// <param name="Bathrooms">Number of bathrooms (supports 1.5 etc.).</param>
/// <param name="IsAccessible">Whether the home is accessible to people with disabilities.</param>
/// <param name="IncomeLimitPercentOfAmi">The income limit as a percent of AMI, or null for no income limit.</param>
/// <param name="SquareFootage">Total square footage.</param>
/// <param name="YearBuilt">Year the home was built.</param>
/// <param name="InitialAssessedValue">Initial assessed value in dollars.</param>
/// <param name="InitialAssessmentDate">The date of the initial assessment.</param>
/// <param name="AppraiserName">The name of the appraiser.</param>
/// <param name="RegisteredBy">The staff user registering the home.</param>
public sealed record RegisterHome(
    HomeId HomeId,
    LandParcelId LandParcelId,
    string? UnitDesignation,
    HomeType UnitType,
    int Bedrooms,
    decimal Bathrooms,
    bool IsAccessible,
    int? IncomeLimitPercentOfAmi,
    decimal SquareFootage,
    int YearBuilt,
    decimal InitialAssessedValue,
    DateOnly InitialAssessmentDate,
    string? AppraiserName,
    UserId RegisteredBy);

/// <summary>Handles <see cref="RegisterHome"/> by creating and persisting a new Home.</summary>
public static class RegisterHomeHandler
{
    /// <summary>Resolves today's date in the trust's time zone, which dates the initial assessment.</summary>
    public static Task<DateOnly> LoadAsync(
        RegisterHome command,
        ITrustClock trustClock,
        IMessageContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(trustClock);
        ArgumentNullException.ThrowIfNull(context);

        return trustClock.TodayAsync(context.TenantId!, cancellationToken);
    }

    /// <summary>Starts the new home's stream with its initial assessment.</summary>
    public static IStartStream Handle(RegisterHome command, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(command);

        var initialAssessment = UnitAssessment.Create(
            command.InitialAssessedValue,
            command.InitialAssessmentDate,
            command.AppraiserName,
            conditionNotes: null,
            command.RegisteredBy,
            TimeProviderContext.Current.UtcNow,
            today);

        var registered = Home.Register(
            command.HomeId,
            command.LandParcelId,
            command.UnitDesignation,
            command.UnitType,
            command.Bedrooms,
            command.Bathrooms,
            command.IsAccessible,
            command.IncomeLimitPercentOfAmi,
            command.SquareFootage,
            command.YearBuilt,
            initialAssessment,
            command.RegisteredBy);

        return MartenOps.StartStream<Home>(command.HomeId.Value, registered);
    }
}
