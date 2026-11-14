using System.Linq.Expressions;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Specifications;

namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>
/// A purchase workflow that has not reached a terminal phase (<see cref="WorkflowPhase.IsTerminal"/>).
/// Stalled counts as in progress: recording the next fact resumes it.
/// </summary>
/// <typeparam name="TView">The purchase workflow read model.</typeparam>
public sealed class PurchaseWorkflowInProgress<TView> : Specification<TView>
    where TView : IPurchaseWorkflowPhaseView
{
    /// <summary>The single instance; the rule has no parameters.</summary>
    public static readonly PurchaseWorkflowInProgress<TView> Instance = new();

    private static readonly string[] TerminalPhases =
        WorkflowPhase.GetAll().Where(p => p.IsTerminal).Select(p => p.DisplayName).ToArray();

    private PurchaseWorkflowInProgress()
    {
    }

    /// <inheritdoc />
    public override Expression<Func<TView, bool>> ToExpression() =>
        v => !TerminalPhases.Contains(v.Phase);
}
