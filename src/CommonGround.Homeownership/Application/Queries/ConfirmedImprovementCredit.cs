using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.ImprovementRequests;
using CommonGround.SharedKernel.Domain;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>The credit a lease has confirmed for finished improvements.</summary>
public static class ConfirmedImprovementCredit
{
    /// <summary>Sums the confirmed credit of the lease's completed requests.</summary>
    public static async Task<Money> ForLeaseAsync(IQuerySession session, string groundLeaseId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        var completed = ImprovementRequestStatus.Completed.DisplayName;
        var requests = await session.Query<ImprovementRequestView>()
            .Where(v => v.GroundLeaseId == groundLeaseId && v.Status == completed)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Money.RoundedFrom(requests.Sum(r => r.ConfirmedCredit ?? 0m));
    }
}
