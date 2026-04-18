using CommonGround.PropertyRegistry.Domain.Buildings.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.PropertyRegistry.Domain.Buildings;

/// <summary>
/// A physical building on a CLT land parcel. Has its own lifecycle independent of occupancy.
/// Homeowners own the building; the CLT retains the land.
/// </summary>
public sealed class Building : AggregateRoot<BuildingId>
{
#pragma warning disable CS8618
    private Building() { }
#pragma warning restore CS8618

    /// <summary>Gets the ID of the land parcel this building sits on.</summary>
    public LandParcels.LandParcelId LandParcelId { get; private set; }

    /// <summary>Gets the physical type of this building.</summary>
    public BuildingType Type { get; private set; }

    /// <summary>Gets the number of bedrooms.</summary>
    public int Bedrooms { get; private set; }

    /// <summary>Gets the number of bathrooms (supports half baths, e.g. 1.5).</summary>
    public decimal Bathrooms { get; private set; }

    /// <summary>Gets the total square footage.</summary>
    public decimal SquareFootage { get; private set; }

    /// <summary>Gets the year the building was built.</summary>
    public int YearBuilt { get; private set; }

    /// <summary>Gets the most recent assessed value.</summary>
    public decimal AssessedValue { get; private set; }

    /// <summary>Gets the date of the most recent assessment, if any.</summary>
    public DateOnly? LastAssessedDate { get; private set; }

    /// <summary>Gets optional condition notes from the most recent assessment.</summary>
    public string? ConditionNotes { get; private set; }

    /// <summary>Gets the current status of the building.</summary>
    public BuildingStatus Status { get; private set; }

    /// <summary>Gets the ID of the user who registered this building.</summary>
    public UserId RegisteredBy { get; private set; }

    /// <summary>Registers a new building on a CLT land parcel.</summary>
    public static Building Register(
        LandParcels.LandParcelId landParcelId,
        BuildingType type,
        int bedrooms,
        decimal bathrooms,
        decimal squareFootage,
        int yearBuilt,
        decimal initialAssessedValue,
        UserId registeredBy)
    {
        ArgumentNullException.ThrowIfNull(landParcelId);
        ArgumentNullException.ThrowIfNull(type);
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

        var structure = new Building();

        structure.ApplyChange(new BuildingRegistered(
            BuildingId.Generate().Value,
            landParcelId.Value,
            type.DisplayName,
            bedrooms,
            bathrooms,
            squareFootage,
            yearBuilt,
            initialAssessedValue,
            registeredBy.Value,
            DateTimeOffset.UtcNow));

        return structure;
    }

    /// <summary>Changes the structure's status. Idempotent if already in the target status.</summary>
    public void ChangeStatus(BuildingStatus newStatus, UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(newStatus);
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status == newStatus)
            return;

        ApplyChange(new BuildingStatusChanged(Id.Value, newStatus.DisplayName, changedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Records a new assessed value for this structure.</summary>
    public void RecordAssessment(decimal assessedValue, DateOnly assessmentDate, string? conditionNotes, UserId assessedBy)
    {
        ArgumentNullException.ThrowIfNull(assessedBy);

        if (assessedValue < 0)
            throw new BusinessRuleViolationException("Assessed value cannot be negative.");

        ApplyChange(new BuildingAssessed(Id.Value, assessedValue, assessmentDate, conditionNotes, assessedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case BuildingRegistered e: Apply(e); break;
            case BuildingStatusChanged e: Apply(e); break;
            case BuildingAssessed e: Apply(e); break;
            default: throw new InvalidOperationException($"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(BuildingRegistered e)
    {
        Id = BuildingId.Create(e.AggregateId);
        LandParcelId = LandParcels.LandParcelId.Create(e.LandParcelId);
        Type = BuildingType.FromDisplayName(e.BuildingType);
        Bedrooms = e.Bedrooms;
        Bathrooms = e.Bathrooms;
        SquareFootage = e.SquareFootage;
        YearBuilt = e.YearBuilt;
        AssessedValue = e.InitialAssessedValue;
        Status = BuildingStatus.Available;
        RegisteredBy = UserId.Create(e.RegisteredBy);
    }

    private void Apply(BuildingStatusChanged e)
    {
        Status = BuildingStatus.FromDisplayName(e.NewStatus);
    }

    private void Apply(BuildingAssessed e)
    {
        AssessedValue = e.AssessedValue;
        LastAssessedDate = e.AssessmentDate;
        ConditionNotes = e.ConditionNotes;
    }
}