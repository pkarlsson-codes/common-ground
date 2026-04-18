using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Persistence;
using Marten;
using Microsoft.Extensions.Logging;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence;

/// <summary>Marten-backed repository for <see cref="LandParcel"/> aggregates.</summary>
internal sealed class LandParcelRepository(
    IDocumentSession session,
    ILogger<LandParcelRepository> logger)
    : MartenRepository<LandParcel, LandParcelId>(session, logger),
    ILandParcelRepository
{
}
