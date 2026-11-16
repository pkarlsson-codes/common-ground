using System.Linq.Expressions;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Specifications;

namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>
/// An application on the waiting list: approved and not yet selected for a home. Approval puts a household
/// on the list; selection, denial or withdrawal takes it off, and a purchase that falls through puts it back.
/// </summary>
public sealed class OnWaitingList<TView> : Specification<TView>
    where TView : IHomeownerApplicationPhaseView
{
    /// <summary>The single instance; the rule has no parameters.</summary>
    public static readonly OnWaitingList<TView> Instance = new();

    private static readonly string ApprovedPhase = ApplicationPhase.Approved.DisplayName;

    private OnWaitingList()
    {
    }

    /// <inheritdoc />
    public override Expression<Func<TView, bool>> ToExpression() =>
        v => v.Phase == ApprovedPhase;
}
