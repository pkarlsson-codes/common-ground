using CommonGround.SharedKernel.Persistence;
using CommonGround.Stewardship.Domain.StewardshipCases;
using Marten;
using Microsoft.Extensions.Logging;

namespace CommonGround.Stewardship.Infrastructure.Persistence;

internal sealed class StewardshipCaseRepository(
    IDocumentSession session,
    IQuerySession querySession,
    ILogger<StewardshipCaseRepository> logger)
    : MartenRepository<StewardshipCase, StewardshipCaseId>(session, logger),
      IStewardshipCaseRepository
{
    public async Task<StewardshipCase?> GetActiveByGroundLeaseIdAsync(
        string groundLeaseId,
        CancellationToken cancellationToken = default)
    {
        var view = await querySession.Query<StewardshipCaseView>()
            .Where(v => v.GroundLeaseId == groundLeaseId && v.Status == nameof(StewardshipCaseStatus.Active))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (view is null)
            return null;

        return await GetByIdAsync(StewardshipCaseId.Create(view.Id), cancellationToken)
            .ConfigureAwait(false);
    }
}
