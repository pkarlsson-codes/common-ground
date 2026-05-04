using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;
using CommonGround.TrustManagement.Domain.Trusts;
using Marten;
using Microsoft.Extensions.Logging;

namespace CommonGround.TrustManagement.Infrastructure.Persistence;

/// <summary>Repository implementation for managing trust entities using Marten.</summary>
public sealed class TrustRepository(
        IDocumentSession session,
        ILogger<MartenRepository<Trust, TrustId>> logger
) : MartenRepository<Trust, TrustId>(session, logger), ITrustRepository
{
}