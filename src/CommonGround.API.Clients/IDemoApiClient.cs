using CommonGround.API.Contracts.Demo.Responses;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for the demo mode API endpoints.</summary>
public interface IDemoApiClient
{
    /// <summary>Gets the demo banner for the caller's trust. The API answers 404 when demo mode is off.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The trust's delete date and the feedback link.</returns>
    Task<DemoResponse> GetDemoAsync(CancellationToken ct = default);
}
