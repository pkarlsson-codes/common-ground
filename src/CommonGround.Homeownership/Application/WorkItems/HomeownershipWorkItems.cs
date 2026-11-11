using System.Globalization;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Application.WorkItems;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Application.WorkItems;

/// <summary>Derives the work items for purchases and lease fee notices from Homeownership's read models.</summary>
public static class HomeownershipWorkItems
{
    /// <summary>How far ahead a scheduled closing counts as coming up.</summary>
    public const int ClosingWindowDays = 14;

    /// <summary>An open purchase waiting on its next steps.</summary>
    public const string PurchaseStepWaiting = "purchase-step-waiting";

    /// <summary>A stalled purchase, due by its resolution target.</summary>
    public const string PurchaseStalled = "purchase-stalled";

    /// <summary>A purchase whose closing is coming up.</summary>
    public const string ClosingScheduled = "closing-scheduled";

    /// <summary>Record the buyer's eligibility.</summary>
    public const string RecordEligibility = "record-eligibility";

    /// <summary>Record the home's appraisal.</summary>
    public const string RecordAppraisal = "record-appraisal";

    /// <summary>Record the buyer's financing commitment.</summary>
    public const string RecordFinancing = "record-financing";

    /// <summary>Record the home inspection.</summary>
    public const string RecordInspection = "record-inspection";

    /// <summary>Record that title has cleared.</summary>
    public const string RecordTitle = "record-title";

    /// <summary>Record that the ground lease was signed.</summary>
    public const string RecordLeaseExecution = "record-lease-execution";

    /// <summary>Schedule the closing, once every step is recorded.</summary>
    public const string ScheduleClosing = "schedule-closing";

    /// <summary>Record that the purchase closed.</summary>
    public const string RecordClosing = "record-closing";

    /// <summary>An active lease with a late lease fee notice due.</summary>
    public const string LeaseFeeNoticeDue = "lease-fee-notice-due";

    /// <summary>Print the late notice and record it as sent.</summary>
    public const string RecordLeaseFeeNotice = "record-lease-fee-notice";

    /// <summary>
    /// The item for a lease with a late notice due: it has waited since the day its oldest unpaid charge passed the
    /// notice's days.
    /// </summary>
    /// <param name="balance">The lease's balance; its <see cref="LeaseFeeBalance.NoticeDue"/> must be set.</param>
    /// <param name="schedule">The trust's notice schedule.</param>
    /// <returns>The lease's work item.</returns>
    public static WorkItem ForLeaseFeeNotice(LeaseFeeBalance balance, LeaseFeeNoticeSchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(balance);
        ArgumentNullException.ThrowIfNull(schedule);

        var level = balance.NoticeDue ?? throw new ArgumentException("The lease has no notice due.", nameof(balance));
        var statement = balance.Statement;
        var days = level == LeaseFeeNoticeLevel.Second ? schedule.SecondNoticeDays : schedule.FirstNoticeDays;
        var levelName = level == LeaseFeeNoticeLevel.Second ? "second" : "first";
        var amount = statement.Balance.ToString("C", CultureInfo.GetCultureInfo("en-US"));

        return new WorkItem(
            WorkArea.Stewardship, LeaseFeeNoticeDue, "ground-lease", balance.Lease.Id,
            balance.HomeLabel ?? "Home",
            $"{balance.Lease.HomeownerFullName}: {levelName} notice due, {amount} owed, {statement.DaysOverdue} days overdue",
            null,
            statement.OldestUnpaidDueDate!.Value.AddDays(days),
            [RecordLeaseFeeNotice]);
    }

    /// <summary>
    /// The one item for an open purchase: stalled, closing within <see cref="ClosingWindowDays"/> of
    /// <paramref name="today"/>, or otherwise waiting on the steps it hasn't recorded since it last moved on.
    /// </summary>
    /// <param name="workflow">The open purchase.</param>
    /// <param name="stepEnteredAt">When the purchase last moved on.</param>
    /// <param name="homeLabel">The label of the home being bought, if it has arrived.</param>
    /// <param name="today">The trust's today.</param>
    /// <param name="zone">The trust's time zone, for the date the purchase last moved on.</param>
    /// <returns>The purchase's work item.</returns>
    public static WorkItem ForPurchase(
        PurchaseWorkflowView workflow,
        DateTimeOffset stepEnteredAt,
        string? homeLabel,
        DateOnly today,
        IanaTimeZone zone)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(zone);

        var label = homeLabel ?? "Home";
        var stepEnteredOn = zone.ToDate(stepEnteredAt);

        if (workflow.Phase == WorkflowPhase.Stalled.DisplayName)
        {
            return new WorkItem(
                WorkArea.Purchases, PurchaseStalled, "purchase", workflow.Id, label,
                $"{workflow.BuyerFullName}: stalled, {workflow.StalledReason}", workflow.ResolutionTargetDate,
                workflow.StalledDate ?? stepEnteredOn, NextActions(workflow));
        }

        if (workflow.ScheduledClosingDate is { } closing && closing <= today.AddDays(ClosingWindowDays))
        {
            return new WorkItem(
                WorkArea.Purchases, ClosingScheduled, "purchase", workflow.Id, label,
                $"{workflow.BuyerFullName}: closing", closing, closing.AddDays(-ClosingWindowDays), [RecordClosing]);
        }

        return new WorkItem(
            WorkArea.Purchases, PurchaseStepWaiting, "purchase", workflow.Id, label,
            $"{workflow.BuyerFullName}: {Waiting(workflow)}", null, stepEnteredOn, NextActions(workflow));
    }

    private static string Waiting(PurchaseWorkflowView workflow)
    {
        if (workflow.ScheduledClosingDate is not null)
            return "waiting for the closing date";

        var missing = MissingSteps(workflow);
        return missing.Count == 0 ? "ready to schedule closing" : $"waiting on {string.Join(", ", missing.Select(m => m.Step))}";
    }

    // Recording a step also resumes a stalled purchase, so a stalled one is fixed the same way as a waiting one.
    private static List<string> NextActions(PurchaseWorkflowView workflow)
    {
        if (workflow.ScheduledClosingDate is not null)
            return [RecordClosing];

        var missing = MissingSteps(workflow);
        return missing.Count == 0 ? [ScheduleClosing] : [.. missing.Select(m => m.Action)];
    }

    private static List<(string Step, string Action)> MissingSteps(PurchaseWorkflowView workflow)
    {
        var missing = new List<(string Step, string Action)>();
        if (workflow.EligibilityDetails is null)
            missing.Add(("eligibility", RecordEligibility));
        if (workflow.AppraisalDetails is null)
            missing.Add(("appraisal", RecordAppraisal));
        if (workflow.FinancingDetails is null)
            missing.Add(("financing", RecordFinancing));
        if (workflow.InspectionResult is null)
            missing.Add(("inspection", RecordInspection));
        if (workflow.TitleDetails is null)
            missing.Add(("title", RecordTitle));
        if (workflow.LeaseExecutionDetails is null)
            missing.Add(("lease signing", RecordLeaseExecution));
        return missing;
    }
}
