using System.Linq.Expressions;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Specifications;

namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>
/// An application that has not reached a terminal phase (<see cref="ApplicationPhase.IsTerminal"/>).
/// Selected counts as open: the purchase can still fall through and return it to the waiting list.
/// </summary>
public sealed class ApplicationOpen<TView> : Specification<TView>
    where TView : IHomeownerApplicationPhaseView
{
    /// <summary>The single instance; the rule has no parameters.</summary>
    public static readonly ApplicationOpen<TView> Instance = new();

    private static readonly string[] TerminalPhases =
        ApplicationPhase.GetAll().Where(p => p.IsTerminal).Select(p => p.DisplayName).ToArray();

    private ApplicationOpen()
    {
    }

    /// <inheritdoc />
    public override Expression<Func<TView, bool>> ToExpression() =>
        v => !TerminalPhases.Contains(v.Phase);
}
