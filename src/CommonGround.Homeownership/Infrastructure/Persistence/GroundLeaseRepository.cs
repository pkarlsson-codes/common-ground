using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.SharedKernel.Persistence;
using Marten;
using Microsoft.Extensions.Logging;

namespace CommonGround.Homeownership.Infrastructure.Persistence;

/// <summary>Marten-backed repository for ground lease aggregates.</summary>
internal sealed class GroundLeaseRepository(
    IDocumentSession session,
    ILogger<GroundLeaseRepository> logger)
    : MartenRepository<GroundLease, GroundLeaseId>(session, logger),
      IGroundLeaseRepository;
