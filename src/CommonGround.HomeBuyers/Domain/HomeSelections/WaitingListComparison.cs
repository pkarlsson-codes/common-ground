using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.HomeSelections;

/// <summary>
/// The waiting list compared against a home on a date: each application with the home's income limit for its
/// household's size and the reasons, if any, it can't be chosen on that date. Applications keep the waiting
/// list's order, longest waiting first; they are not scored or ranked.
/// </summary>
public sealed class WaitingListComparison
{
    private WaitingListComparison(
        DateOnly asOf,
        int? incomeLimitPercentOfAmi,
        IncomeLimitTable? incomeLimits,
        IReadOnlyList<ComparedApplication> applications)
    {
        AsOf = asOf;
        IncomeLimitPercentOfAmi = incomeLimitPercentOfAmi;
        IncomeLimits = incomeLimits;
        Applications = applications;
    }

    /// <summary>The date the applications are judged on.</summary>
    public DateOnly AsOf { get; }

    /// <summary>The home's income limit as a percent of AMI, or <see langword="null"/> when it has none.</summary>
    public int? IncomeLimitPercentOfAmi { get; }

    /// <summary>The income limit schedule in effect on <see cref="AsOf"/>, or <see langword="null"/> when none is.</summary>
    public IncomeLimitTable? IncomeLimits { get; }

    /// <summary>The schedule's row for the home's percent, when the home has a limit and the schedule has it.</summary>
    public IncomeLimitLevel? IncomeLimitLevel =>
        IncomeLimitPercentOfAmi is { } percent ? IncomeLimits?.LevelFor(percent) : null;

    /// <summary>
    /// Whether the applications can be judged against the home's income limit: the home has none, or the schedule
    /// in effect has its level. A household can't be chosen otherwise.
    /// </summary>
    public bool HasIncomeLimitsInEffect => IncomeLimitPercentOfAmi is null || IncomeLimitLevel is not null;

    /// <summary>Every application on the waiting list, longest waiting first.</summary>
    public IReadOnlyList<ComparedApplication> Applications { get; }

    /// <summary>
    /// Compares <paramref name="waiting"/> against a home on <paramref name="asOf"/>.
    /// </summary>
    /// <param name="waiting">The waiting list, in the order it is to be shown.</param>
    /// <param name="incomeLimitPercentOfAmi">The home's income limit, or <see langword="null"/> when it has none.</param>
    /// <param name="incomeLimits">The income limit schedule in effect on <paramref name="asOf"/>, if any.</param>
    /// <param name="asOf">The date to judge the applications on.</param>
    /// <returns>The comparison.</returns>
    public static WaitingListComparison Create(
        IReadOnlyList<WaitingApplication> waiting,
        int? incomeLimitPercentOfAmi,
        IncomeLimitTable? incomeLimits,
        DateOnly asOf)
    {
        ArgumentNullException.ThrowIfNull(waiting);

        var level = incomeLimitPercentOfAmi is { } percent ? incomeLimits?.LevelFor(percent) : null;
        return new WaitingListComparison(
            asOf,
            incomeLimitPercentOfAmi,
            incomeLimits,
            [.. waiting.Select(application => Compare(application, level?.LimitFor(application.HouseholdSize), asOf))]);
    }

    /// <summary>The application on the waiting list, or <see langword="null"/> when it isn't on it.</summary>
    public ComparedApplication? Find(HomeownerApplicationId applicationId) =>
        Applications.FirstOrDefault(a => a.Application.ApplicationId == applicationId);

    private static ComparedApplication Compare(WaitingApplication application, Money? incomeLimit, DateOnly asOf)
    {
        var reasons = new List<CannotBeChosenReason>();
        if (incomeLimit is not null && application.AnnualIncome > incomeLimit)
            reasons.Add(CannotBeChosenReason.OverIncomeLimit);
        if (!application.HasPrimaryMember)
            reasons.Add(CannotBeChosenReason.NoPrimaryMember);
        if (!application.IsHousingCounselingCompleted)
            reasons.Add(CannotBeChosenReason.HousingCounselingIncomplete);
        if (!application.IsCltOrientationCompleted)
            reasons.Add(CannotBeChosenReason.CltOrientationIncomplete);
        if (application.PrequalificationExpiryDate is not { } expiry)
            reasons.Add(CannotBeChosenReason.NoPrequalification);
        else if (expiry < asOf)
            reasons.Add(CannotBeChosenReason.PrequalificationExpired);
        if (application.QualificationToConfirmSince is not null)
            reasons.Add(CannotBeChosenReason.QualificationToConfirm);

        return new ComparedApplication(application, incomeLimit, reasons);
    }
}

/// <summary>An application on the waiting list as compared against a home.</summary>
/// <param name="Application">The application and what its household has on record.</param>
/// <param name="IncomeLimit">The home's income limit for the household's size, or <see langword="null"/> when it has none or none is in effect.</param>
/// <param name="Reasons">Why the household can't be chosen; empty when it can.</param>
public sealed record ComparedApplication(
    WaitingApplication Application,
    Money? IncomeLimit,
    IReadOnlyList<CannotBeChosenReason> Reasons)
{
    /// <summary>Whether the household can be chosen.</summary>
    public bool CanBeChosen => Reasons.Count == 0;
}
