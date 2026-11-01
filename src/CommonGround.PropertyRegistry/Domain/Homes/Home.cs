using CommonGround.PropertyRegistry.Domain.Homes.Events;
using CommonGround.PropertyRegistry.Application.Errors;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
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

    /// <summary>Gets the current occupancy status of this home.</summary>
    public HomeStatus Status { get; private set; }

    /// <summary>Gets the assessment history for this home, oldest first.</summary>
    public IReadOnlyList<UnitAssessment> Assessments => _assessments;

    /// <summary>Registers a new home on a CLT land parcel.</summary>
    public static Home Register(
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
        ArgumentNullException.ThrowIfNull(landParcelId);
        ArgumentNullException.ThrowIfNull(unitType);
        ArgumentNullException.ThrowIfNull(initialAssessment);
        ArgumentNullException.ThrowIfNull(registeredBy);

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

        var home = new Home();

        home.ApplyChange(new HomeRegistered(
            HomeId.Generate().Value,
            landParcelId.Value,
            string.IsNullOrWhiteSpace(unitDesignation) ? null : unitDesignation.Trim(),
            unitType.DisplayName,
            bedrooms,
            bathrooms,
            isAccessible,
            squareFootage,
            yearBuilt,
            initialAssessment.AssessedValue,
            initialAssessment.AssessedAt,
            initialAssessment.AppraiserName,
            registeredBy.Value));

        return home;
    }

    /// <summary>
    /// Records that a home selection has started choosing a household for this home. A home under
    /// contract only gets a new selection once its purchase has fallen through, and news of that may
    /// arrive after the selection's, so the contract is released here first.
    /// </summary>
    public void OpenSelection(string homeSelectionId, DateOnly openedDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeSelectionId);

        if (Status == HomeStatus.UnderContract)
            ReleaseContract(openedDate);

        if (Status != HomeStatus.Available)
            throw BusinessRuleViolation.Home.OpenSelection.HomeNotAvailable(Status);

        ApplyChange(new HomeSelectionOpened(Id.Value, homeSelectionId, openedDate));
    }

    /// <summary>Records that the home's selection was cancelled without choosing anyone, making the home available again.</summary>
    public void CloseSelectionWithoutSale(string homeSelectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeSelectionId);

        if (Status != HomeStatus.InSelection)
            throw BusinessRuleViolation.Home.CloseSelectionWithoutSale.HomeNotInSelection(Status);

        ApplyChange(new HomeSelectionClosedWithoutSale(Id.Value, homeSelectionId));
    }

    /// <summary>Marks the home under contract to the household selected for it.</summary>
    public void MarkUnderContract(DateOnly contractDate)
    {
        if (Status != HomeStatus.Available && Status != HomeStatus.InSelection)
            throw BusinessRuleViolation.Home.MarkUnderContract.HomeNotAvailable();

        ApplyChange(new HomeUnderContract(Id.Value, contractDate));
    }

    /// <summary>Makes the home available again after its purchase failed or was withdrawn.</summary>
    public void ReleaseContract(DateOnly releasedDate)
    {
        if (Status != HomeStatus.UnderContract)
            throw BusinessRuleViolation.Home.ReleaseContract.HomeNotUnderContract(Status);

        ApplyChange(new HomeContractReleased(Id.Value, releasedDate));
    }

    /// <summary>
    /// Marks the home leased once its purchase closes. A purchase started directly, rather than
    /// from a home selection, closes without the home ever having been under contract.
    /// </summary>
    public void MarkLeased(DateOnly leasedDate)
    {
        if (Status == HomeStatus.Leased || Status == HomeStatus.Resale)
            throw BusinessRuleViolation.Home.MarkLeased.HomeAlreadyLeased(Status);

        ApplyChange(new HomeLeased(Id.Value, leasedDate));
    }

    /// <summary>Records a new assessed value for this unit.</summary>
    public void RecordAssessment(decimal assessedValue, DateOnly assessmentDate, string? appraiserName, string? conditionNotes, UserId recordedById)
    {
        ArgumentNullException.ThrowIfNull(recordedById);

        if (assessedValue < 0)
            throw BusinessRuleViolation.Home.RecordAssessment.AssessedValueNegative(assessedValue);

        ApplyChange(new HomeAssessed(Id.Value, assessedValue, assessmentDate, appraiserName, conditionNotes, recordedById.Value));
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case HomeRegistered e: Apply(e); break;
            case HomeAssessed e: Apply(e); break;
            case HomeSelectionOpened e: Apply(e); break;
            case HomeSelectionClosedWithoutSale e: Apply(e); break;
            case HomeUnderContract e: Apply(e); break;
            case HomeContractReleased e: Apply(e); break;
            case HomeLeased e: Apply(e); break;
            default: throw new InvalidOperationException($"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(HomeRegistered e)
    {
        Id = HomeId.Create(e.AggregateId);
        LandParcelId = LandParcels.LandParcelId.Create(e.LandParcelId);
        UnitDesignation = e.UnitDesignation;
        UnitType = HomeType.FromDisplayName(e.UnitType);
        Bedrooms = e.Bedrooms;
        Bathrooms = e.Bathrooms;
        IsAccessible = e.IsAccessible;
        SquareFootage = e.SquareFootage;
        YearBuilt = e.YearBuilt;
        Status = HomeStatus.Available;
        RegisteredBy = UserId.Create(e.RegisteredBy);
        _assessments.Add(UnitAssessment.Reconstitute(
            e.InitialAssessedValue,
            e.InitiallyAssessedAt,
            e.InitialAppraiserName,
            conditionNotes: null,
            RegisteredBy,
            e.OccurredAt));
    }

    private void Apply(HomeAssessed e)
    {
        _assessments.Add(UnitAssessment.Reconstitute(e.AssessedValue, e.AssessmentDate, e.AppraiserName, e.ConditionNotes, UserId.Create(e.RecordedById), e.OccurredAt));
    }

    private void Apply(HomeSelectionOpened _) => Status = HomeStatus.InSelection;

    private void Apply(HomeSelectionClosedWithoutSale _) => Status = HomeStatus.Available;

    private void Apply(HomeUnderContract _) => Status = HomeStatus.UnderContract;

    private void Apply(HomeContractReleased _) => Status = HomeStatus.Available;

    private void Apply(HomeLeased _) => Status = HomeStatus.Leased;
}
