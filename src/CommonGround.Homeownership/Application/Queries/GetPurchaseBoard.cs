using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns every open purchase, grouped by the step it waits on.</summary>
public sealed record GetPurchaseBoard;

/// <summary>An open purchase on the board.</summary>
/// <param name="Workflow">The purchase.</param>
/// <param name="HomeLabel">The label of the home being bought, if it has arrived.</param>
/// <param name="StepEnteredOn">The trust date the purchase last moved on.</param>
public sealed record PurchaseBoardCard(PurchaseWorkflowListItemView Workflow, string? HomeLabel, DateOnly StepEnteredOn);

/// <summary>The purchases waiting on one step, longest waiting first.</summary>
/// <param name="Step">The <see cref="PurchaseStep"/> name.</param>
/// <param name="Cards">The purchases waiting on it.</param>
public sealed record PurchaseBoardColumn(string Step, IReadOnlyList<PurchaseBoardCard> Cards);

/// <summary>The board: one column per step, in step order, empty ones included.</summary>
/// <param name="Columns">The columns.</param>
public sealed record PurchaseBoard(IReadOnlyList<PurchaseBoardColumn> Columns);

/// <summary>Handles <see cref="GetPurchaseBoard"/>.</summary>
public static class GetPurchaseBoardHandler
{
    private static readonly string[] TerminalPhaseNames =
    [
        WorkflowPhase.Closed.DisplayName,
        WorkflowPhase.Failed.DisplayName,
        WorkflowPhase.Withdrawn.DisplayName,
    ];

    /// <summary>Loads the open purchases and their home labels and puts each in its step's column.</summary>
    public static async Task<PurchaseBoard> Handle(
        GetPurchaseBoard request,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var zone = await trustClock.GetTimeZoneAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        var workflows = await session.Query<PurchaseWorkflowListItemView>()
            .Where(v => !TerminalPhaseNames.Contains(v.Phase))
            .OrderBy(v => v.StepEnteredAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var labels = (await session
            .LoadManyAsync<HomeLabelView>(cancellationToken, workflows.Select(w => w.HomeId).Distinct())
            .ConfigureAwait(false))
            .ToDictionary(v => v.Id, v => v.Label);

        return new PurchaseBoard([
            .. PurchaseStep.GetAll().OrderBy(s => s.Value).Select(step => new PurchaseBoardColumn(
                step.DisplayName,
                [
                    .. workflows
                        .Where(w => w.NextStep == step.DisplayName)
                        .Select(w => new PurchaseBoardCard(w, labels.GetValueOrDefault(w.HomeId), zone.ToDate(w.StepEnteredAt))),
                ])),
        ]);
    }
}
