using CommonGround.API.Contracts.PortfolioImport;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for the portfolio import API endpoints.</summary>
public sealed class PortfolioImportApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<PortfolioImportApiClient> logger) : BaseApiClient(httpClientFactory.CreateClient(nameof(PortfolioImportApiClient)), logger), IPortfolioImportApiClient
{
    /// <inheritdoc/>
    public async Task<PortfolioImportResponse> CheckAsync(PortfolioImportRequest request, CancellationToken ct = default)
        => await PostAsync<PortfolioImportRequest, PortfolioImportResponse>("api/portfolio-import/check", request, ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<PortfolioImportResponse> ImportAsync(PortfolioImportRequest request, CancellationToken ct = default)
        => await PostAsync<PortfolioImportRequest, PortfolioImportResponse>("api/portfolio-import", request, ct).ConfigureAwait(false);
}
