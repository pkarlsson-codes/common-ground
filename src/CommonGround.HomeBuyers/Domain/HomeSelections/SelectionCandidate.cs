using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.HomeSelections;

/// <summary>
/// An approved application on the waiting list, frozen as it stood when a home selection
/// started. Staff compare candidates on these facts, so later changes to the application
/// or household don't change what the selection was based on.
/// </summary>
public sealed class SelectionCandidate : ValueObject
{
    private SelectionCandidate(
        HomeownerApplicationId applicationId,
        HouseholdId householdId,
        Money annualIncome,
        int householdSize,
        bool requiresAccessibility,
        DateOnly waitingSince)
    {
        ApplicationId = applicationId;
        HouseholdId = householdId;
        AnnualIncome = annualIncome;
        HouseholdSize = householdSize;
        RequiresAccessibility = requiresAccessibility;
        WaitingSince = waitingSince;
    }

    /// <summary>Creates and validates a new <see cref="SelectionCandidate"/>.</summary>
    public static SelectionCandidate Create(
        HomeownerApplicationId applicationId,
        HouseholdId householdId,
        Money annualIncome,
        int householdSize,
        bool requiresAccessibility,
        DateOnly waitingSince)
    {
        ArgumentNullException.ThrowIfNull(annualIncome);
        if (annualIncome.IsNegative)
            throw new ArgumentOutOfRangeException(nameof(annualIncome), annualIncome, "Annual income can't be negative.");
        ArgumentOutOfRangeException.ThrowIfLessThan(householdSize, 1);

        return new SelectionCandidate(
            applicationId, householdId, annualIncome, householdSize, requiresAccessibility, waitingSince);
    }

    /// <summary>The candidate's application.</summary>
    public HomeownerApplicationId ApplicationId { get; }

    /// <summary>The household that applied.</summary>
    public HouseholdId HouseholdId { get; }

    /// <summary>The household's annual income as declared on the application.</summary>
    public Money AnnualIncome { get; }

    /// <summary>The number of people in the household as declared on the application.</summary>
    public int HouseholdSize { get; }

    /// <summary>Whether the household needs an accessible home.</summary>
    public bool RequiresAccessibility { get; }

    /// <summary>The date the household joined the waiting list.</summary>
    public DateOnly WaitingSince { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ApplicationId;
        yield return HouseholdId;
        yield return AnnualIncome;
        yield return HouseholdSize;
        yield return RequiresAccessibility;
        yield return WaitingSince;
    }
}
