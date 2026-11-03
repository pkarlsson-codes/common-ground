using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Application.WorkItems;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Application.WorkItems;

/// <summary>Derives the work items for applicants and home selections from HomeBuyers' read models.</summary>
public static class HomeBuyersWorkItems
{
    /// <summary>How far ahead a mortgage prequalification counts as expiring.</summary>
    public const int PrequalificationWindowDays = 30;

    /// <summary>A submitted application waiting for staff to approve or deny it.</summary>
    public const string ApplicationAwaitingDecision = "application-awaiting-decision";

    /// <summary>A household with an open application that hasn't finished readiness.</summary>
    public const string ReadinessStepMissing = "readiness-step-missing";

    /// <summary>A household with an open application whose prequalification expires soon.</summary>
    public const string PrequalificationExpiring = "prequalification-expiring";

    /// <summary>A home selection that hasn't chosen a household yet.</summary>
    public const string SelectionOpen = "selection-open";

    /// <summary>Approve a submitted application.</summary>
    public const string ApproveApplication = "approve-application";

    /// <summary>Deny a submitted application.</summary>
    public const string DenyApplication = "deny-application";

    /// <summary>Record that the household completed housing counseling.</summary>
    public const string RecordHousingCounseling = "record-housing-counseling";

    /// <summary>Record that the household completed CLT orientation.</summary>
    public const string RecordCltOrientation = "record-clt-orientation";

    /// <summary>Record the household's mortgage prequalification.</summary>
    public const string RecordMortgagePrequalification = "record-mortgage-prequalification";

    /// <summary>Choose the household a selection goes to.</summary>
    public const string ChooseHousehold = "choose-household";

    /// <summary>
    /// The items for an open (submitted or approved) application: a decision while it is submitted, and its
    /// household's missing readiness or expiring prequalification as of <paramref name="today"/>. Readiness of
    /// households that haven't applied isn't anyone's work yet.
    /// </summary>
    /// <param name="application">The application.</param>
    /// <param name="household">The household that applied, if its view has arrived.</param>
    /// <param name="today">The trust's today.</param>
    /// <param name="zone">The trust's time zone, for the date the application was submitted.</param>
    /// <returns>The application's work items.</returns>
    public static IEnumerable<WorkItem> ForApplication(
        HomeownerApplicationListItemView application,
        HouseholdListItemView? household,
        DateOnly today,
        IanaTimeZone zone)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(zone);

        var label = household?.PrimaryMemberName ?? "Household";
        var submittedOn = zone.ToDate(application.SubmittedAt);

        if (application.Phase == ApplicationPhase.Submitted.DisplayName)
        {
            yield return new WorkItem(
                WorkArea.Applicants, ApplicationAwaitingDecision, "application", application.Id, label,
                "Submitted, waiting for a decision", null, submittedOn,
                [ApproveApplication, DenyApplication]);
        }

        if (household is null)
            yield break;

        var missing = MissingReadiness(household, today);
        if (missing.Count > 0)
        {
            yield return new WorkItem(
                WorkArea.Applicants, ReadinessStepMissing, "household", household.Id, label,
                $"Missing {string.Join(", ", missing.Select(m => m.Step))}", null, submittedOn,
                [.. missing.Select(m => m.Action)]);
        }
        else if (household.MortgagePrequalificationExpiryDate is { } expiry && expiry <= today.AddDays(PrequalificationWindowDays))
        {
            yield return new WorkItem(
                WorkArea.Applicants, PrequalificationExpiring, "household", household.Id, label,
                "Mortgage prequalification expiring", expiry, expiry.AddDays(-PrequalificationWindowDays),
                [RecordMortgagePrequalification]);
        }
    }

    /// <summary>The item for a selection that is still open.</summary>
    /// <param name="selection">The open selection.</param>
    /// <param name="homeLabel">The label of the home being selected for, if its facts have arrived.</param>
    /// <param name="zone">The trust's time zone, for the date the selection started.</param>
    /// <returns>The selection's work item.</returns>
    public static WorkItem ForSelection(HomeSelectionView selection, string? homeLabel, IanaTimeZone zone)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(zone);

        var candidates = selection.Candidates.Count == 1 ? "1 candidate" : $"{selection.Candidates.Count} candidates";
        return new WorkItem(
            WorkArea.Selections, SelectionOpen, "selection", selection.Id, homeLabel ?? "Home",
            $"Open, {candidates} to choose from", null, zone.ToDate(selection.StartedAt), [ChooseHousehold]);
    }

    private static List<(string Step, string Action)> MissingReadiness(HouseholdListItemView household, DateOnly today)
    {
        var missing = new List<(string Step, string Action)>();
        if (!household.IsHousingCounselingCompleted)
            missing.Add(("housing counseling", RecordHousingCounseling));
        if (!household.IsCltOrientationCompleted)
            missing.Add(("CLT orientation", RecordCltOrientation));
        if (!(household.MortgagePrequalificationExpiryDate >= today))
        {
            missing.Add((
                household.NeedsReverificationOn(today) ? "a current mortgage prequalification" : "mortgage prequalification",
                RecordMortgagePrequalification));
        }

        return missing;
    }
}
