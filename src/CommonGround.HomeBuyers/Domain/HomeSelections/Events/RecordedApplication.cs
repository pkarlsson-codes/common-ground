using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.HomeSelections.Events;

/// <summary>
/// One application on the waiting list when a household was chosen for a home, as it was compared then, and what
/// the choice decided for it. <paramref name="IncomeLimit"/> is the home's limit for the household's size, null when
/// the home has none. <paramref name="Reasons"/> say why the household couldn't be chosen; empty unless
/// <paramref name="Outcome"/> is <see cref="SelectionOutcome.CouldNotBeChosen"/>.
/// </summary>
public sealed record RecordedApplication(
    string ApplicationId,
    string HouseholdId,
    Money AnnualIncome,
    int HouseholdSize,
    bool RequiresAccessibility,
    DateOnly WaitingSince,
    Money? IncomeLimit,
    SelectionOutcome Outcome,
    IReadOnlyList<CannotBeChosenReason> Reasons);
