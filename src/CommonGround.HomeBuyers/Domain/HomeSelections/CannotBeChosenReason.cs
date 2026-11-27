using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.HomeSelections;

/// <summary>Why an application on the waiting list couldn't be chosen for a home on a given date.</summary>
public sealed class CannotBeChosenReason : Enumeration<CannotBeChosenReason>
{
    /// <summary>The household's income is over the home's income limit for its size.</summary>
    public static readonly CannotBeChosenReason OverIncomeLimit = new(1, nameof(OverIncomeLimit));

    /// <summary>The household has no primary member to buy the home.</summary>
    public static readonly CannotBeChosenReason NoPrimaryMember = new(2, nameof(NoPrimaryMember));

    /// <summary>The household hasn't completed housing counseling.</summary>
    public static readonly CannotBeChosenReason HousingCounselingIncomplete = new(3, nameof(HousingCounselingIncomplete));

    /// <summary>The household hasn't completed CLT orientation.</summary>
    public static readonly CannotBeChosenReason CltOrientationIncomplete = new(4, nameof(CltOrientationIncomplete));

    /// <summary>The household has no mortgage prequalification on record.</summary>
    public static readonly CannotBeChosenReason NoPrequalification = new(5, nameof(NoPrequalification));

    /// <summary>The household's mortgage prequalification expired before the date.</summary>
    public static readonly CannotBeChosenReason PrequalificationExpired = new(6, nameof(PrequalificationExpired));

    /// <summary>The household's income or size changed after approval, and staff haven't confirmed it still qualifies.</summary>
    public static readonly CannotBeChosenReason QualificationToConfirm = new(7, nameof(QualificationToConfirm));

    private CannotBeChosenReason(int value, string displayName) : base(value, displayName) { }
}
