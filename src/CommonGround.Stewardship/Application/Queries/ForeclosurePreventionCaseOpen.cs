using System.Linq.Expressions;
using CommonGround.SharedKernel.Specifications;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;
using CommonGround.Stewardship.Infrastructure.Persistence;

namespace CommonGround.Stewardship.Application.Queries;

/// <summary>
/// A foreclosure prevention case that is not closed for good (<see cref="ForeclosurePreventionCaseStatus.IsTerminal"/>).
/// Both Opened and Active count as open.
/// </summary>
public sealed class ForeclosurePreventionCaseOpen : Specification<ForeclosurePreventionCaseView>
{
    /// <summary>The single instance; the rule has no parameters.</summary>
    public static readonly ForeclosurePreventionCaseOpen Instance = new();

    private static readonly string[] TerminalStatuses =
        ForeclosurePreventionCaseStatus.GetAll().Where(s => s.IsTerminal).Select(s => s.DisplayName).ToArray();

    private ForeclosurePreventionCaseOpen()
    {
    }

    /// <inheritdoc />
    public override Expression<Func<ForeclosurePreventionCaseView, bool>> ToExpression() =>
        v => !TerminalStatuses.Contains(v.Status);
}
