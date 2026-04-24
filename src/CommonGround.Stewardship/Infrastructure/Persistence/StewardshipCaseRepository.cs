using CommonGround.SharedKernel.Persistence;
using CommonGround.Stewardship.Domain.StewardshipCases;
using Marten;
using Microsoft.Extensions.Logging;

namespace CommonGround.Stewardship.Infrastructure.Persistence;

internal sealed class StewardshipCaseRepository(
    IDocumentSession session,
    ILogger<StewardshipCaseRepository> logger)
    : MartenRepository<StewardshipCase, StewardshipCaseId>(session, logger),
      IStewardshipCaseRepository;
