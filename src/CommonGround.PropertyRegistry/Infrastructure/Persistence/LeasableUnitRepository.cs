using CommonGround.PropertyRegistry.Domain.LeasableUnits;
using CommonGround.SharedKernel.Persistence;
using Marten;
using Microsoft.Extensions.Logging;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence;

internal sealed class LeasableUnitRepository : MartenRepository<LeasableUnit, LeasableUnitId>, ILeasableUnitRepository
{
    private readonly IDocumentSession _session;

    public LeasableUnitRepository(
        IDocumentSession session,
        ILogger<LeasableUnitRepository> logger)
        : base(session, logger)
    {
        _session = session;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Tries to reconstruct a <see cref="DwellingUnit"/> first; falls back to <see cref="CommercialUnit"/>.
    /// Returns null if neither stream exists for the given ID.
    /// </remarks>
    public override async Task<LeasableUnit?> GetByIdAsync(LeasableUnitId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var dwelling = await _session.Events.FetchForWriting<DwellingUnit>(id.Value, cancellationToken).ConfigureAwait(false);
        if (dwelling.Aggregate is not null)
        {
            return dwelling.Aggregate;
        }

        var commercial = await _session.Events.FetchForWriting<CommercialUnit>(id.Value, cancellationToken).ConfigureAwait(false);
        return commercial.Aggregate;
    }
}
