using CommonGround.HomeBuyers.Domain.HomeownerApplications;

namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>
/// Where a household is on its way to owning a CLT home, and the date it reached each step so far. The steps
/// are Intake → Ready → Approved → Waiting → Selected → Purchasing → Homeowner; Approved and Selected are
/// moments, so a household is only ever at the other five.
/// </summary>
/// <param name="Stage">The household's current stage: Intake, Ready, Waiting, Purchasing or Homeowner.</param>
/// <param name="ReadyDate">When readiness was completed, if it is complete.</param>
/// <param name="ApprovedDate">When the current application was approved, from Waiting on.</param>
/// <param name="SelectedDate">When the current application was selected for a home, from Purchasing on.</param>
/// <param name="PurchasedDate">When the purchase closed, once a Homeowner.</param>
/// <param name="CurrentApplicationId">The application the stage comes from, from Waiting on, or the one awaiting a decision.</param>
/// <param name="SelectedHomeId">The home the household was selected for, from Purchasing on.</param>
public sealed record HouseholdJourney(
    string Stage,
    DateOnly? ReadyDate,
    DateOnly? ApprovedDate,
    DateOnly? SelectedDate,
    DateOnly? PurchasedDate,
    string? CurrentApplicationId,
    string? SelectedHomeId)
{
    public const string Intake = nameof(Intake);
    public const string Ready = nameof(Ready);
    public const string Waiting = nameof(Waiting);
    public const string Purchasing = nameof(Purchasing);
    public const string Homeowner = nameof(Homeowner);

    /// <summary>
    /// Derives the journey from the household and its <paramref name="applications"/> as of <paramref name="today"/>.
    /// A household whose purchase fell through is approved again, so it is back to Waiting; one whose
    /// application was denied or withdrawn is back to Ready, or Intake if its readiness has since lapsed.
    /// <paramref name="prequalificationRecordedDate"/> is the trust date the current prequalification was recorded.
    /// </summary>
    public static HouseholdJourney Derive(
        HouseholdView household,
        IReadOnlyList<HomeownerApplicationListItemView> applications,
        DateOnly? prequalificationRecordedDate,
        DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(household);
        ArgumentNullException.ThrowIfNull(applications);

        var readyDate = ReadyDateOf(household, prequalificationRecordedDate, today);

        var purchased = applications.FirstOrDefault(a => a.Phase == ApplicationPhase.Purchased.DisplayName);
        if (purchased is not null)
            return new HouseholdJourney(Homeowner, readyDate, purchased.ApprovedDate, purchased.SelectedDate,
                purchased.PurchasedDate, purchased.Id, purchased.SelectedHomeId);

        var selected = applications.FirstOrDefault(a => a.Phase == ApplicationPhase.Selected.DisplayName);
        if (selected is not null)
            return new HouseholdJourney(Purchasing, readyDate, selected.ApprovedDate, selected.SelectedDate,
                null, selected.Id, selected.SelectedHomeId);

        var approved = applications.FirstOrDefault(OnWaitingList<HomeownerApplicationListItemView>.Instance.IsSatisfiedBy);
        if (approved is not null)
            return new HouseholdJourney(Waiting, readyDate, approved.ApprovedDate, null, null, approved.Id, null);

        var submitted = applications.FirstOrDefault(a => a.Phase == ApplicationPhase.Submitted.DisplayName);
        return new HouseholdJourney(readyDate is null ? Intake : Ready, readyDate, null, null, null, submitted?.Id, null);
    }

    private static DateOnly? ReadyDateOf(HouseholdView household, DateOnly? prequalificationRecordedDate, DateOnly today)
    {
        var prequalification = household.MortgagePrequalification;
        if (household.HousingCounselingCompletedDate is not { } counseled
            || household.CltOrientationCompletedDate is not { } oriented
            || prequalification is null
            || prequalification.ExpiryDate < today
            || prequalificationRecordedDate is not { } prequalified)
            return null;

        return new[] { counseled, oriented, prequalified }.Max();
    }
}
