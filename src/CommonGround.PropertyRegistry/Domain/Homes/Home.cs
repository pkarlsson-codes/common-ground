using CommonGround.PropertyRegistry.Domain.Homes.Events;
using CommonGround.PropertyRegistry.Application.Errors;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;

namespace CommonGround.PropertyRegistry.Domain.Homes;

/// <summary>
/// A residential home on a CLT land parcel.
/// Homeowners own the unit; the CLT retains the land via a ground lease.
/// </summary>
public sealed class Home : AggregateRoot<HomeId>
{
    private readonly List<UnitAssessment> _assessments = [];

#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private Home() { }
#pragma warning restore CS8618

    /// <summary>Gets the ID of the land parcel this unit sits on.</summary>
    public LandParcels.LandParcelId LandParcelId { get; private set; }

    /// <summary>Gets the total square footage.</summary>
    public decimal SquareFootage { get; private set; }

    /// <summary>Gets the year the unit was built.</summary>
    public int YearBuilt { get; private set; }

    /// <summary>Gets the ID of the user who registered this unit.</summary>
    public UserId RegisteredBy { get; private set; }

    /// <summary>Gets the unit designation (e.g. "Unit B") that tells this home apart from others on its parcel, if any.</summary>
    public string? UnitDesignation { get; private set; }

    /// <summary>Gets the physical type of this home.</summary>
    public HomeType UnitType { get; private set; }

    /// <summary>Gets the number of bedrooms.</summary>
    public int Bedrooms { get; private set; }

    /// <summary>Gets the number of bathrooms (supports half baths, e.g. 1.5).</summary>
    public decimal Bathrooms { get; private set; }

    /// <summary>Gets a value indicating whether the home is accessible to people with disabilities.</summary>
    public bool IsAccessible { get; private set; }

    /// <summary>Gets the assessment history for this home, oldest first.</summary>
    public IReadOnlyList<UnitAssessment> Assessments => _assessments;

    /// <summary>Registers a new home on a CLT land parcel.</summary>
    public static HomeRegistered Register(
        HomeId id,
        LandParcels.LandParcelId landParcelId,
        string? unitDesignation,
        HomeType unitType,
        int bedrooms,
        decimal bathrooms,
        bool isAccessible,
        decimal squareFootage,
        int yearBuilt,
        UnitAssessment initialAssessment,
        UserId registeredBy)
    {
        ArgumentNullException.ThrowIfNull(unitType);
        ArgumentNullException.ThrowIfNull(initialAssessment);

        var minYear = 1800;
        var maxYear = TimeProviderContext.Current.UtcNow.Year + 5;

        if (bedrooms < 0)
            throw BusinessRuleViolation.Home.Register.BedroomsNegative(bedrooms);

        if (bathrooms <= 0)
            throw BusinessRuleViolation.Home.Register.BathroomsNonPositive(bathrooms);

        if (squareFootage <= 0)
            throw BusinessRuleViolation.Home.Register.SquareFootageNonPositive(squareFootage);

        if (yearBuilt < minYear || yearBuilt > maxYear)
            throw BusinessRuleViolation.Home.Register.YearBuiltOutOfRange(yearBuilt, minYear, maxYear);

        return new HomeRegistered(
            id.Value,
            landParcelId.Value,
            string.IsNullOrWhiteSpace(unitDesignation) ? null : unitDesignation.Trim(),
            unitType,
            bedrooms,
            bathrooms,
            isAccessible,
            squareFootage,
            yearBuilt,
            initialAssessment.AssessedValue,
            initialAssessment.AssessedAt,
            initialAssessment.AppraiserName,
            registeredBy.Value);
    }

    /// <summary>Records a new assessed value for this unit.</summary>
    public HomeAssessed RecordAssessment(decimal assessedValue, DateOnly assessmentDate, string? appraiserName, string? conditionNotes, UserId recordedById)
    {
        if (assessedValue < 0)
            throw BusinessRuleViolation.Home.RecordAssessment.AssessedValueNegative(assessedValue);

        return new HomeAssessed(Id.Value, assessedValue, assessmentDate, appraiserName, conditionNotes, recordedById.Value);
    }

    /// <summary>Starts a home from its first event.</summary>
    public static Home Create(HomeRegistered e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var home = new Home();
        home.Apply(e);
        return home;
    }

    private void Apply(HomeRegistered e)
    {
        Id = HomeId.Create(e.AggregateId);
        LandParcelId = LandParcels.LandParcelId.Create(e.LandParcelId);
        UnitDesignation = e.UnitDesignation;
        UnitType = e.UnitType;
        Bedrooms = e.Bedrooms;
        Bathrooms = e.Bathrooms;
        IsAccessible = e.IsAccessible;
        SquareFootage = e.SquareFootage;
        YearBuilt = e.YearBuilt;
        RegisteredBy = UserId.Create(e.RegisteredBy);
        _assessments.Add(UnitAssessment.Reconstitute(
            e.InitialAssessedValue,
            e.InitiallyAssessedAt,
            e.InitialAppraiserName,
            conditionNotes: null,
            RegisteredBy,
            e.OccurredAt));
    }

    /// <summary>Applies <see cref="HomeAssessed"/>.</summary>
    public void Apply(HomeAssessed e)
    {
        _assessments.Add(UnitAssessment.Reconstitute(e.AssessedValue, e.AssessmentDate, e.AppraiserName, e.ConditionNotes, UserId.Create(e.RecordedById), e.OccurredAt));
    }
}
