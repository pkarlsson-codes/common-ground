using System.Net;

namespace CommonGround.SharedKernel.Infrastructure.API;

/// <summary>
/// Exception thrown when an upstream API returns a non-success response that should be surfaced to callers.
/// </summary>
public sealed class UpstreamApiException : HttpRequestException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpstreamApiException"/> class.
    /// </summary>
    /// <param name="endpoint">The upstream endpoint that returned the failure.</param>
    /// <param name="statusCode">The HTTP status code returned by the upstream service.</param>
    /// <param name="responseBody">The raw response body returned by the upstream service.</param>
    /// <param name="contentType">The upstream response content type, if available.</param>
    public UpstreamApiException(string endpoint, HttpStatusCode statusCode, string? responseBody, string? contentType)
        : base($"Upstream API request to '{endpoint}' failed with status code {(int)statusCode} ({statusCode}).", null, statusCode)
    {
        Endpoint = endpoint;
        ResponseBody = responseBody;
        ContentType = contentType;
    }

    /// <summary>Gets the upstream endpoint that failed.</summary>
    public string Endpoint { get; }

    /// <summary>Gets the raw response body returned by the upstream service.</summary>
    public string? ResponseBody { get; }

    /// <summary>Gets the upstream response content type, if known.</summary>
    public string? ContentType { get; }
}