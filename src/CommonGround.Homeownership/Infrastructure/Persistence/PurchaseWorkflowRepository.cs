using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.SharedKernel.Persistence;
using Marten;
using Microsoft.Extensions.Logging;

namespace CommonGround.Homeownership.Infrastructure.Persistence;

/// <summary>Marten-backed repository for purchase workflow aggregates.</summary>
internal sealed class PurchaseWorkflowRepository(
    IDocumentSession session,
    ILogger<PurchaseWorkflowRepository> logger)
    : MartenRepository<PurchaseWorkflow, PurchaseWorkflowId>(session, logger),
      IPurchaseWorkflowRepository;
