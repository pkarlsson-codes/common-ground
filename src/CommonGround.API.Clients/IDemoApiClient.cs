using CommonGround.API.Contracts.Demo.Requests;
using CommonGround.API.Contracts.Demo.Responses;

namespace CommonGround.API.Clients;

/// <summary>HTTP client for the demo mode API endpoints.</summary>
public interface IDemoApiClient
{
    /// <summary>Gets whether this server is a demo server. Needs no signed-in user. The API answers 404 when demo mode is off.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>How long a sample trust lives.</returns>
    Task<DemoModeResponse> GetModeAsync(CancellationToken ct = default);

    /// <summary>Gets the demo banner for the caller's trust. The API answers 404 when demo mode is off.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The trust's delete date.</returns>
    Task<DemoResponse> GetDemoAsync(CancellationToken ct = default);

    /// <summary>Saves the caller's feedback. The API answers 404 when demo mode is off or the trust is not a sandbox.</summary>
    /// <param name="request">What they wrote, the page they were on, and whether they may be replied to.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task.</returns>
    Task SendFeedbackAsync(SendDemoFeedbackRequest request, CancellationToken ct = default);

    /// <summary>Starts a sample trust for the caller, who has no trust yet. The API answers 404 when demo mode is off.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The sandbox being filled.</returns>
    Task<DemoSandboxResponse> StartSandboxAsync(CancellationToken ct = default);

    /// <summary>Gets the caller's latest sandbox. The API answers 404 when demo mode is off.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The sandbox, if the caller started one.</returns>
    Task<CurrentDemoSandboxResponse> GetCurrentSandboxAsync(CancellationToken ct = default);
}
