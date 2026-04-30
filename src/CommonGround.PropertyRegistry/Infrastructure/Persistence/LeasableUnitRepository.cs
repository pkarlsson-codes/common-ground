using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.SharedKernel.Persistence;
using Marten;
using Microsoft.Extensions.Logging;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence;

internal sealed class HomeRepository(
    IDocumentSession session,
    ILogger<HomeRepository> logger)
    : MartenRepository<Home, HomeId>(session, logger),
    IHomeRepository
{
}