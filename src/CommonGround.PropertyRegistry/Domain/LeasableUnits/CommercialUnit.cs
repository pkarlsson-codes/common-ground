using CommonGround.PropertyRegistry.Domain.LeasableUnits.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits;

/// <summary>
/// A commercial unit on a CLT land parcel.
/// </summary>
public sealed class CommercialUnit : LeasableUnit
{
#pragma warning disable CS8618
    private CommercialUnit() { }
#pragma warning restore CS8618

    /// <summary>Gets the type of this commercial unit.</summary>
    public CommercialUnitType UnitType { get; private set; }

    /// <summary>Registers a new commercial unit on a CLT land parcel.</summary>
    public static CommercialUnit Register(
        LandParcels.LandParcelId landParcelId,
        decimal squareFootage,
        int yearBuilt,
        decimal initialAssessedValue,
        UserId registeredBy)
    {
        ArgumentNullException.ThrowIfNull(landParcelId);
        ArgumentNullException.ThrowIfNull(registeredBy);

        if (squareFootage <= 0)
            throw new BusinessRuleViolationException("Square footage must be greater than zero.");

        if (yearBuilt < 1800 || yearBuilt > DateTimeOffset.UtcNow.Year + 5)
            throw new BusinessRuleViolationException($"Year built must be between 1800 and {DateTimeOffset.UtcNow.Year + 5}.");

        if (initialAssessedValue < 0)
            throw new BusinessRuleViolationException("Assessed value cannot be negative.");

        var unit = new CommercialUnit();

        unit.ApplyChange(new CommercialUnitRegistered(
            LeasableUnitId.Generate().Value,
            landParcelId.Value,
            squareFootage,
            yearBuilt,
            initialAssessedValue,
            registeredBy.Value,
            DateTimeOffset.UtcNow));

        return unit;
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case CommercialUnitRegistered e: Apply(e); break;
            case LeasableUnitAssessed e: ApplyAssessed(e); break;
            default: throw new InvalidOperationException($"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(CommercialUnitRegistered e)
    {
        Id = LeasableUnitId.Create(e.AggregateId);
        LandParcelId = LandParcels.LandParcelId.Create(e.LandParcelId);
        UnitType = CommercialUnitType.Retail;
        SquareFootage = e.SquareFootage;
        YearBuilt = e.YearBuilt;
        AssessedValue = e.InitialAssessedValue;
        RegisteredBy = UserId.Create(e.RegisteredBy);
    }
}
