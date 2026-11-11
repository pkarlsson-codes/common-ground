using CommonGround.API.Contracts.PortfolioImport;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for the portfolio import API endpoints.</summary>
public interface IPortfolioImportApiClient
{
    /// <summary>Checks an import file without changing anything.</summary>
    /// <param name="request">The file's text.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>Every row error, and what importing the file would create and skip.</returns>
    Task<PortfolioImportResponse> CheckAsync(PortfolioImportRequest request, CancellationToken ct = default);

    /// <summary>Imports a file. Throws <c>UpstreamApiException</c> with status 422 when any row has an error.</summary>
    /// <param name="request">The file's text.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>What was created and what already existed.</returns>
    Task<PortfolioImportResponse> ImportAsync(PortfolioImportRequest request, CancellationToken ct = default);
}
