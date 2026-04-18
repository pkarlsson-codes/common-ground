using CommonGround.PropertyRegistry.Domain.Buildings;
using CommonGround.SharedKernel.Persistence;
using Marten;
using Microsoft.Extensions.Logging;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence;

internal sealed class StructureRepository(
    IDocumentSession session,
    ILogger<StructureRepository> logger)
    : MartenRepository<Building, BuildingId>(session, logger),
    IBuildingRepository
{
}
