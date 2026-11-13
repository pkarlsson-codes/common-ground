using CommonGround.API.Contracts.Reports.Responses;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for report API endpoints.</summary>
public interface IReportsApiClient
{
    /// <summary>Gets the funder report for a period.</summary>
    /// <param name="firstDay">The first day of the period.</param>
    /// <param name="lastDay">The last day of the period.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The homes on the books on <paramref name="lastDay"/>, the sales and the buybacks in the period.</returns>
    Task<FunderReportResponse> GetFunderReportAsync(DateOnly? firstDay, DateOnly? lastDay, CancellationToken cancellationToken);
}
