using CommonGround.SharedKernel.Persistence;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;
using Marten;
using Microsoft.Extensions.Logging;

namespace CommonGround.Stewardship.Infrastructure.Persistence;

internal sealed class ForeclosurePreventionCaseRepository(
    IDocumentSession session,
    ILogger<ForeclosurePreventionCaseRepository> logger)
    : MartenRepository<ForeclosurePreventionCase, ForeclosurePreventionCaseId>(session, logger),
      IForeclosurePreventionCaseRepository;
