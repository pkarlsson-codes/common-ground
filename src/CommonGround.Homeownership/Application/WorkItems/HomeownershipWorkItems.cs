using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Application.WorkItems;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Application.WorkItems;

/// <summary>Derives the work items for purchases from Homeownership's read models.</summary>
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
                workflow.StalledDate ?? stepEnteredOn);
        }

        if (workflow.ScheduledClosingDate is { } closing && closing <= today.AddDays(ClosingWindowDays))
        {
            return new WorkItem(
                WorkArea.Purchases, ClosingScheduled, "purchase", workflow.Id, label,
                $"{workflow.BuyerFullName}: closing", closing, closing.AddDays(-ClosingWindowDays));
        }

        return new WorkItem(
            WorkArea.Purchases, PurchaseStepWaiting, "purchase", workflow.Id, label,
            $"{workflow.BuyerFullName}: {Waiting(workflow)}", null, stepEnteredOn);
    }

    private static string Waiting(PurchaseWorkflowView workflow)
    {
        if (workflow.ScheduledClosingDate is not null)
            return "waiting for the closing date";

        var missing = new List<string>();
        if (workflow.EligibilityDetails is null)
            missing.Add("eligibility");
        if (workflow.FinancingDetails is null)
            missing.Add("financing");
        if (workflow.InspectionResult is null)
            missing.Add("inspection");
        if (workflow.TitleDetails is null)
            missing.Add("title");
        if (workflow.LeaseExecutionDetails is null)
            missing.Add("lease signing");

        return missing.Count == 0 ? "ready to schedule closing" : $"waiting on {string.Join(", ", missing)}";
    }
}
