using CommonGround.PropertyRegistry.Domain.LeasableUnits.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits;

/// <summary>
/// A residential home on a CLT land parcel.
/// Homeowners own the unit; the CLT retains the land via a ground lease.
/// </summary>
public sealed class Home : LeasableUnit
{
#pragma warning disable CS8618
    private Home() { }
#pragma warning restore CS8618

    /// <summary>Gets the physical type of this home.</summary>
    public HomeType UnitType { get; private set; }

    /// <summary>Gets the number of bedrooms.</summary>
    public int Bedrooms { get; private set; }

    /// <summary>Gets the number of bathrooms (supports half baths, e.g. 1.5).</summary>
    public decimal Bathrooms { get; private set; }

    /// <summary>Gets the current occupancy status of this home.</summary>
    public UnitStatus Status { get; private set; }

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
            LeasableUnitId.Generate().Value,
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

    /// <summary>Executes a ground lease on this home. Home must be Available.</summary>
    public void ExecuteGroundLease(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status != UnitStatus.Available)
            throw new BusinessRuleViolationException($"Cannot execute a ground lease on a home with status '{Status.DisplayName}'. Home must be Available.");

        ApplyChange(new GroundLeaseExecuted(Id.Value, changedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Confirms move-in for this home. Home must be PendingOccupancy.</summary>
    public void ConfirmMoveIn(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status != UnitStatus.PendingOccupancy)
            throw new BusinessRuleViolationException($"Cannot confirm move-in on a home with status '{Status.DisplayName}'. Home must be PendingOccupancy.");

        ApplyChange(new MoveInConfirmed(Id.Value, changedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Initiates resale for this home. Home must be Occupied.</summary>
    public void InitiateResale(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status != UnitStatus.Occupied)
            throw new BusinessRuleViolationException($"Cannot initiate resale on a home with status '{Status.DisplayName}'. Home must be Occupied.");

        ApplyChange(new ResaleInitiated(Id.Value, changedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Completes resale closing for this home. Home must be in Resale status.</summary>
    public void CompleteResaleClosing(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status != UnitStatus.Resale)
            throw new BusinessRuleViolationException($"Cannot complete resale closing on a home with status '{Status.DisplayName}'. Home must be in Resale status.");

        ApplyChange(new ResaleClosingCompleted(Id.Value, changedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Cancels pending occupancy for this home. Home must be PendingOccupancy.</summary>
    public void CancelOccupancy(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status != UnitStatus.PendingOccupancy)
            throw new BusinessRuleViolationException($"Cannot cancel occupancy on a home with status '{Status.DisplayName}'. Home must be PendingOccupancy.");

        ApplyChange(new OccupancyCancelled(Id.Value, changedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Reopens the resale listing for this home. Home must be in Resale status.</summary>
    public void ReopenResaleListing(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status != UnitStatus.Resale)
            throw new BusinessRuleViolationException($"Cannot reopen resale listing on a home with status '{Status.DisplayName}'. Home must be in Resale status.");

        ApplyChange(new ResaleListingReopened(Id.Value, changedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Abandons resale in favour of redevelopment. Home must be in Resale or PendingOccupancy status.</summary>
    public void AbandonResaleForRedevelopment(UserId changedBy)
    {
        ArgumentNullException.ThrowIfNull(changedBy);

        if (Status != UnitStatus.Resale && Status != UnitStatus.PendingOccupancy)
            throw new BusinessRuleViolationException($"Cannot abandon resale for redevelopment on a home with status '{Status.DisplayName}'. Home must be in Resale or PendingOccupancy status.");

        ApplyChange(new ResaleAbandonedForRedevelopment(Id.Value, changedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case HomeRegistered e: Apply(e); break;
            case LeasableUnitAssessed e: ApplyAssessed(e); break;
            case GroundLeaseExecuted: Status = UnitStatus.PendingOccupancy; break;
            case MoveInConfirmed: Status = UnitStatus.Occupied; break;
            case ResaleInitiated: Status = UnitStatus.Resale; break;
            case ResaleClosingCompleted: Status = UnitStatus.PendingOccupancy; break;
            case OccupancyCancelled: Status = UnitStatus.Available; break;
            case ResaleListingReopened: Status = UnitStatus.Available; break;
            case ResaleAbandonedForRedevelopment: Status = UnitStatus.Available; break;
            default: throw new InvalidOperationException($"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(HomeRegistered e)
    {
        Id = LeasableUnitId.Create(e.AggregateId);
        LandParcelId = LandParcels.LandParcelId.Create(e.LandParcelId);
        UnitType = HomeType.FromDisplayName(e.UnitType);
        Bedrooms = e.Bedrooms;
        Bathrooms = e.Bathrooms;
        SquareFootage = e.SquareFootage;
        YearBuilt = e.YearBuilt;
        AssessedValue = e.InitialAssessedValue;
        Status = UnitStatus.Available;
        RegisteredBy = UserId.Create(e.RegisteredBy);
    }
}
