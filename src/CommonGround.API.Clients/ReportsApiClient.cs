using System.Globalization;
using CommonGround.API.Contracts.Reports.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.Extensions.Logging;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for report API endpoints.</summary>
public sealed class ReportsApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<ReportsApiClient> logger)
    : BaseApiClient(httpClientFactory.CreateClient(nameof(ReportsApiClient)), logger), IReportsApiClient
{
    /// <inheritdoc/>
    public async Task<FunderReportResponse> GetFunderReportAsync(DateOnly? firstDay, DateOnly? lastDay, CancellationToken cancellationToken)
    {
        var query = new QueryStringBuilder()
            .AddIfNotWhiteSpace("from", firstDay?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .AddIfNotWhiteSpace("to", lastDay?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .Build();
        return await GetAsync<FunderReportResponse>($"api/reports/funder{query}", cancellationToken).ConfigureAwait(false);
    }
}
