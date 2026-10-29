using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.HomeSelections;

/// <summary>
/// A reason staff can give for choosing one candidate over another in a home selection.
/// Priorities rank candidates who already qualify; they never decide who qualifies.
/// </summary>
public sealed class SelectionPriority : Enumeration<SelectionPriority>
{
    /// <summary>The household has a lower annual income than other candidates.</summary>
    public static readonly SelectionPriority LowerIncome = new(1, nameof(LowerIncome));
    /// <summary>The household's size suits the home's number of bedrooms.</summary>
    public static readonly SelectionPriority HouseholdSizeFit = new(2, nameof(HouseholdSizeFit));
    /// <summary>The household needs an accessible home and this home is accessible.</summary>
    public static readonly SelectionPriority AccessibilityNeed = new(3, nameof(AccessibilityNeed));
    /// <summary>The household has been on the waiting list longer than other candidates.</summary>
    public static readonly SelectionPriority WaitingListTenure = new(4, nameof(WaitingListTenure));

    private SelectionPriority(int value, string displayName) : base(value, displayName) { }
}
