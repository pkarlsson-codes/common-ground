using CommonGround.PropertyRegistry.Domain.Homes.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.PropertyRegistry.Domain.Homes;

/// <summary>
/// A residential home on a CLT land parcel.
/// Homeowners own the unit; the CLT retains the land via a ground lease.
/// </summary>
public sealed class Home : AggregateRoot<HomeId>
{
    private readonly List<UnitAssessment> _assessments = [];

#pragma warning disable CS8618
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

    /// <summary>Gets the physical type of this home.</summary>
    public HomeType UnitType { get; private set; }

    /// <summary>Gets the number of bedrooms.</summary>
    public int Bedrooms { get; private set; }

    /// <summary>Gets the number of bathrooms (supports half baths, e.g. 1.5).</summary>
    public decimal Bathrooms { get; private set; }

    /// <summary>Gets the current occupancy status of this home.</summary>
    public HomeStatus Status { get; private set; }

    /// <summary>Gets the assessment history for this home, oldest first.</summary>
    public IReadOnlyList<UnitAssessment> Assessments => _assessments;

    /// <summary>Registers a new home on a CLT land parcel.</summary>
    public static Home Register(
        LandParcels.LandParcelId landParcelId,
        HomeType unitType,
        int bedrooms,
        decimal bathrooms,
        decimal squareFootage,
        int yearBuilt,
        decimal initialAssessedValue,
        UserId registeredBy)
    {
        ArgumentNullException.ThrowIfNull(landParcelId);
        ArgumentNullException.ThrowIfNull(unitType);
        ArgumentNullException.ThrowIfNull(registeredBy);

        if (bedrooms < 0)
            throw new BusinessRuleViolationException("Bedrooms cannot be negative.");

        if (bathrooms <= 0)
            throw new BusinessRuleViolationException("Bathrooms must be greater than zero.");

        if (squareFootage <= 0)
            throw new BusinessRuleViolationException("Square footage must be greater than zero.");

        if (yearBuilt < 1800 || yearBuilt > DateTimeOffset.UtcNow.Year + 5)
            throw new BusinessRuleViolationException($"Year built must be between 1800 and {DateTimeOffset.UtcNow.Year + 5}.");

        if (initialAssessedValue < 0)
            throw new BusinessRuleViolationException("Assessed value cannot be negative.");

        var home = new Home();

        home.ApplyChange(new HomeRegistered(
            HomeId.Generate().Value,
            landParcelId.Value,
            unitType.DisplayName,
            bedrooms,
            bathrooms,
            squareFootage,
            yearBuilt,
            initialAssessedValue,
            registeredBy.Value,
            DateTimeOffset.UtcNow));

        return home;
    }

    /// <summary>Records a new assessed value for this unit.</summary>
    public void RecordAssessment(decimal assessedValue, DateOnly assessmentDate, string? conditionNotes, UserId assessedBy)
    {
        ArgumentNullException.ThrowIfNull(assessedBy);

        if (assessedValue < 0)
            throw new BusinessRuleViolationException("Assessed value cannot be negative.");

        ApplyChange(new HomeAssessed(Id.Value, assessedValue, assessmentDate, conditionNotes, assessedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case HomeRegistered e: Apply(e); break;
            case HomeAssessed e: Apply(e); break;
            default: throw new InvalidOperationException($"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(HomeRegistered e)
    {
        Id = HomeId.Create(e.AggregateId);
        LandParcelId = LandParcels.LandParcelId.Create(e.LandParcelId);
        UnitType = HomeType.FromDisplayName(e.UnitType);
        Bedrooms = e.Bedrooms;
        Bathrooms = e.Bathrooms;
        SquareFootage = e.SquareFootage;
        YearBuilt = e.YearBuilt;
        Status = HomeStatus.Available;
        RegisteredBy = UserId.Create(e.RegisteredBy);
    }

    private void Apply(HomeAssessed e)
    {
        _assessments.Add(new UnitAssessment(e.AssessedValue, e.AssessmentDate, e.ConditionNotes, UserId.Create(e.AssessedBy), e.OccurredAt));
    }
}
