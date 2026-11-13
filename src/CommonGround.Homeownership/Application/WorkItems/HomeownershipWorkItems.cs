using System.Globalization;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.MortgageRequests;
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

    /// <summary>An active lease whose notice of default's cure deadline passed unpaid (Art 12.1).</summary>
    public const string LeaseDefaultUncured = "lease-default-uncured";

    /// <summary>Decide what to do about the uncured default: end the lease (Art 12.4) or agree a plan.</summary>
    public const string ReviewLeaseDefault = "review-lease-default";

    /// <summary>A homeowner's request for a new loan on the home, waiting for the trust's decision.</summary>
    public const string MortgageRequestPending = "mortgage-request-pending";

    /// <summary>An approved loan whose expected closing has passed and isn't recorded closed or withdrawn.</summary>
    public const string MortgageLoanClosingUnrecorded = "mortgage-loan-closing-unrecorded";

    /// <summary>Add the loan's appraisal, which the debt check needs.</summary>
    public const string RecordMortgageRequestAppraisal = "record-mortgage-request-appraisal";

    /// <summary>Approve or deny the request.</summary>
    public const string DecideMortgageRequest = "decide-mortgage-request";

    /// <summary>Record that the approved loan closed, or that the homeowner withdrew it.</summary>
    public const string RecordMortgageLoanClosed = "record-mortgage-loan-closed";

    /// <summary>
    /// The item for a mortgage request: a pending one from the day it arrived, due on its expected closing; an
    /// approved one from the day after its expected closing, until it is recorded closed or withdrawn. Other requests,
    /// and approved ones whose closing hasn't passed, have none.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="homeLabel">The label of the home, if known.</param>
    /// <param name="today">The trust's today.</param>
    /// <returns>The request's work item, or null when it needs nothing.</returns>
    public static WorkItem? ForMortgageRequest(MortgageRequestView request, string? homeLabel, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(request);

        var usd = CultureInfo.GetCultureInfo("en-US");
        var loan = $"{request.Principal.ToString("C0", usd)} from {request.Lender}";

        if (request.Status == MortgageRequestStatus.Pending.DisplayName)
        {
            var waitingOn = request.DebtCheckMissing() ? "waiting for the appraisal" : "ready to decide";
            return new WorkItem(
                WorkArea.Stewardship, MortgageRequestPending, "ground-lease", request.GroundLeaseId,
                homeLabel ?? "Home",
                $"{request.HomeownerFullName}: mortgage request, {loan}, {waitingOn}",
                request.ExpectedClosingOn,
                request.ReceivedOn,
                [request.DebtCheckMissing() ? RecordMortgageRequestAppraisal : DecideMortgageRequest]);
        }

        if (request.Status == MortgageRequestStatus.Approved.DisplayName && request.ExpectedClosingOn < today)
        {
            return new WorkItem(
                WorkArea.Stewardship, MortgageLoanClosingUnrecorded, "ground-lease", request.GroundLeaseId,
                homeLabel ?? "Home",
                $"{request.HomeownerFullName}: approved loan, {loan}, expected to close {request.ExpectedClosingOn.ToString("MMM d", usd)}",
                null,
                request.ExpectedClosingOn.AddDays(1),
                [RecordMortgageLoanClosed]);
        }

        return null;
    }

    private static bool DebtCheckMissing(this MortgageRequestView request) => request.PurchaseOptionPrice is null;

    /// <summary>
    /// The item for an active lease whose notice of default wasn't cured: due the day after the cure deadline, and
    /// gone once the amount owed is paid or the lease ends.
    /// </summary>
    /// <param name="balance">The lease's balance; its <see cref="LeaseFeeBalance.DefaultCure"/> must be uncured.</param>
    /// <returns>The lease's work item.</returns>
    public static WorkItem ForUncuredDefault(LeaseFeeBalance balance)
    {
        ArgumentNullException.ThrowIfNull(balance);

        var cure = balance.DefaultCure ?? throw new ArgumentException("The lease has no notice of default.", nameof(balance));
        var usd = CultureInfo.GetCultureInfo("en-US");

        return new WorkItem(
            WorkArea.Stewardship, LeaseDefaultUncured, "ground-lease", balance.Lease.Id,
            balance.HomeLabel ?? "Home",
            $"{balance.Lease.HomeownerFullName}: default not cured by {cure.CureBy.ToString("MMM d", usd)}, " +
            $"{cure.StillToPay} of {cure.AmountOwed} still to pay",
            null,
            cure.CureBy.AddDays(1),
            [ReviewLeaseDefault]);
    }

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
        var days = schedule.DaysFor(level);
        var levelName = level == LeaseFeeNoticeLevel.Default ? "notice of default"
            : level == LeaseFeeNoticeLevel.Second ? "second notice"
            : "first notice";
        var amount = statement.Balance;

        return new WorkItem(
            WorkArea.Stewardship, LeaseFeeNoticeDue, "ground-lease", balance.Lease.Id,
            balance.HomeLabel ?? "Home",
            $"{balance.Lease.HomeownerFullName}: {levelName} due, {amount} owed, {statement.DaysOverdue} days overdue",
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
