using CommonGround.PropertyRegistry.Domain.LeasableUnits.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits;

/// <summary>
/// Abstract base for all leasable units on CLT land parcels.
/// Carries shared physical and assessment facts.
/// </summary>
public abstract class LeasableUnit : AggregateRoot<LeasableUnitId>
{
#pragma warning disable CS8618, CS1591
    protected LeasableUnit() { }
#pragma warning restore CS8618, CS1591

    /// <summary>Gets the ID of the land parcel this unit sits on.</summary>
    public LandParcels.LandParcelId LandParcelId { get; protected set; }

    /// <summary>Gets the total square footage.</summary>
    public decimal SquareFootage { get; protected set; }

    /// <summary>Gets the year the unit was built.</summary>
    public int YearBuilt { get; protected set; }

    /// <summary>Gets the most recent assessed value.</summary>
    public decimal AssessedValue { get; protected set; }

    /// <summary>Gets the date of the most recent assessment, if any.</summary>
    public DateOnly? LastAssessedDate { get; protected set; }

    /// <summary>Gets optional condition notes from the most recent assessment.</summary>
    public string? ConditionNotes { get; protected set; }

    /// <summary>Gets the ID of the user who registered this unit.</summary>
    public UserId RegisteredBy { get; protected set; }

    /// <summary>Records a new assessed value for this unit.</summary>
    public void RecordAssessment(decimal assessedValue, DateOnly assessmentDate, string? conditionNotes, UserId assessedBy)
    {
        ArgumentNullException.ThrowIfNull(assessedBy);

        if (assessedValue < 0)
            throw new SharedKernel.Domain.Exceptions.BusinessRuleViolationException("Assessed value cannot be negative.");

        ApplyChange(new LeasableUnitAssessed(Id.Value, assessedValue, assessmentDate, conditionNotes, assessedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Applies the assessed event — shared by all concrete unit types.</summary>
    protected void ApplyAssessed(LeasableUnitAssessed e)
    {
        AssessedValue = e.AssessedValue;
        LastAssessedDate = e.AssessmentDate;
        ConditionNotes = e.ConditionNotes;
    }
}
