using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace CommonGround.SharedKernel.Infrastructure.API;

/// <summary>
/// Base class for API clients that provides common HTTP operations and error handling.
/// </summary>
public abstract partial class BaseApiClient
{
    /// <summary>
    /// Initializes a new instance of the BaseApiClient class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API requests.</param>
    /// <param name="logger">The logger for this client.</param>
    /// <param name="pipeline">Optional resilience pipeline. If null, a default pipeline is created.</param>
    protected BaseApiClient(
        HttpClient httpClient,
        ILogger logger,
        ResiliencePipeline<HttpResponseMessage>? pipeline = null)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        Pipeline = pipeline ?? CreateDefaultPipeline();
    }

    /// <summary>
    /// The HTTP client used for making API requests.
    /// </summary>
    protected HttpClient HttpClient { get; }

    /// <summary>
    /// The logger for this API client.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// JSON serialization options for request/response handling.
    /// </summary>
    protected JsonSerializerOptions JsonOptions { get; }

    /// <summary>
    /// The Polly resilience pipeline for resilient HTTP requests.
    /// </summary>
    protected ResiliencePipeline<HttpResponseMessage> Pipeline { get; }

    /// <summary>
    /// Performs a GET request and deserializes the response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response data to.</typeparam>
    /// <param name="endpoint">The API endpoint to call.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized response data.</returns>
    protected Task<T> GetAsync<T>(
        string endpoint,
        CancellationToken cancellationToken)
        where T : class
        => ExecuteAsync<T>(
            endpoint,
            ctx => HttpClient.GetAsync(endpoint, ctx.CancellationToken),
            cancellationToken);

    /// <summary>
    /// Performs a POST request and deserializes the response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request data.</typeparam>
    /// <typeparam name="TResponse">The type to deserialize the response data to.</typeparam>
    /// <param name="endpoint">The API endpoint to call.</param>
    /// <param name="request">The request data to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized response data.</returns>
    protected Task<TResponse> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : class
        where TResponse : class
    {
        ArgumentNullException.ThrowIfNull(request);
        return ExecuteAsync<TResponse>(
            endpoint,
            ctx => HttpClient.PostAsJsonAsync(endpoint, request, JsonOptions, ctx.CancellationToken),
            cancellationToken);
    }

    /// <summary>
    /// Performs a PUT request and deserializes the response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request data.</typeparam>
    /// <typeparam name="TResponse">The type to deserialize the response data to.</typeparam>
    /// <param name="endpoint">The API endpoint to call.</param>
    /// <param name="request">The request data to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized response data.</returns>
    protected Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class
    {
        ArgumentNullException.ThrowIfNull(request);
        return ExecuteAsync<TResponse>(
            endpoint,
            ctx => HttpClient.PutAsJsonAsync(endpoint, request, JsonOptions, ctx.CancellationToken),
            cancellationToken);
    }

    /// <summary>
    /// Performs a DELETE request.
    /// </summary>
    /// <param name="endpoint">The API endpoint to call.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
        => ExecuteAsync(
            endpoint,
            ctx => HttpClient.DeleteAsync(endpoint, ctx.CancellationToken),
            cancellationToken);

    /// <summary>
    /// Performs a POST request with a request body but no response body (e.g., 204 No Content).
    /// </summary>
    /// <typeparam name="TRequest">The type of the request data.</typeparam>
    /// <param name="endpoint">The API endpoint to call.</param>
    /// <param name="request">The request data to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task PostAsync<TRequest>(string endpoint, TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class
    {
        ArgumentNullException.ThrowIfNull(request);
        return ExecuteAsync(
            endpoint,
            ctx => HttpClient.PostAsJsonAsync(endpoint, request, JsonOptions, ctx.CancellationToken),
            cancellationToken);
    }

    /// <summary>
    /// Performs a POST request with no request body.
    /// </summary>
    /// <param name="endpoint">The API endpoint to call.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task PostAsync(string endpoint, CancellationToken cancellationToken = default)
        => ExecuteAsync(
            endpoint,
            ctx => HttpClient.PostAsync(endpoint, null, ctx.CancellationToken),
            cancellationToken);

    /// <summary>
    /// Performs a POST request with no request body and deserializes the response.
    /// </summary>
    /// <typeparam name="TResponse">The type to deserialize the response data to.</typeparam>
    /// <param name="endpoint">The API endpoint to call.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized response data.</returns>
    protected Task<TResponse> PostAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        where TResponse : class
        => ExecuteAsync<TResponse>(
            endpoint,
            ctx => HttpClient.PostAsync(endpoint, null, ctx.CancellationToken),
            cancellationToken);

    /// <summary>
    /// Performs a PUT request with no request body.
    /// </summary>
    /// <param name="endpoint">The API endpoint to call.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task PutAsync(string endpoint, CancellationToken cancellationToken = default)
        => ExecuteAsync(
            endpoint,
            ctx => HttpClient.PutAsync(endpoint, null, ctx.CancellationToken),
            cancellationToken);

    /// <summary>
    /// Sends an HTTP DELETE request to the specified endpoint and returns the deserialized response data.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response data expected from the API.</typeparam>
    /// <param name="endpoint">The URI of the endpoint to which the DELETE request is sent. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>The deserialized response data of type <typeparamref name="TResponse"/> returned by the API.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the API response data is null.</exception>
    protected Task<TResponse> DeleteAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        where TResponse : class
        => ExecuteAsync<TResponse>(
            endpoint,
            ctx => HttpClient.DeleteAsync(endpoint, ctx.CancellationToken),
            cancellationToken);

    /// <summary>
    /// Performs a POST request with FormData and deserializes the response.
    /// </summary>
    /// <typeparam name="TResponse">The type to deserialize the response data to.</typeparam>
    /// <param name="endpoint">The API endpoint to call.</param>
    /// <param name="formData">The FormData content to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized response data.</returns>
    protected Task<TResponse> PostFormAsync<TResponse>(
        string endpoint,
        HttpContent formData,
        CancellationToken cancellationToken)
        where TResponse : class
    {
        ArgumentNullException.ThrowIfNull(formData);
        return ExecuteAsync<TResponse>(
            endpoint,
            ctx => HttpClient.PostAsync(endpoint, formData, ctx.CancellationToken),
            cancellationToken);
    }

    /// <summary>
    /// Performs a PATCH request with a request body and no response body (e.g., 204 No Content).
    /// </summary>
    /// <typeparam name="TRequest">The type of the request data.</typeparam>
    /// <param name="endpoint">The API endpoint to call.</param>
    /// <param name="request">The request data to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task PatchAsync<TRequest>(string endpoint, TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class
    {
        ArgumentNullException.ThrowIfNull(request);
        return ExecuteAsync(
            endpoint,
            ctx => HttpClient.PatchAsJsonAsync(endpoint, request, JsonOptions, ctx.CancellationToken),
            cancellationToken);
    }

    /// <summary>
    /// Performs a GET request and returns the raw response stream.
    /// </summary>
    /// <param name="endpoint">The API endpoint to call.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response content stream.</returns>
    protected async Task<Stream> GetStreamAsync(
        string endpoint,
        CancellationToken cancellationToken)
    {
        var response = await ExecuteAsync(
            endpoint,
            ctx => HttpClient.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead, ctx.CancellationToken),
            cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a default resilience pipeline with retry and timeout for resilient HTTP requests.
    /// </summary>
    /// <returns>A default resilience pipeline.</returns>
    private static ResiliencePipeline<HttpResponseMessage> CreateDefaultPipeline()
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>()
                    .HandleResult(response =>
                        !response.IsSuccessStatusCode &&
                        (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                        (int)response.StatusCode >= 500)),
            })
            .AddTimeout(TimeSpan.FromSeconds(30))
            .Build();
    }

    /// <summary>
    /// Core scaffolding: acquires a ResilienceContext, executes the pipeline, ensures success,
    /// and releases the context. Catches and logs HttpRequestException.
    /// </summary>
    private async Task<HttpResponseMessage> ExecuteAsync(
        string endpoint,
        Func<ResilienceContext, Task<HttpResponseMessage>> httpFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var context = ResilienceContextPool.Shared.Get(cancellationToken);
            try
            {
                context.Properties.Set(new ResiliencePropertyKey<string>("uri"), endpoint);
                var response = await Pipeline
                    .ExecuteAsync(async ctx => await httpFactory(ctx).ConfigureAwait(false), context)
                    .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                return response;
            }
            finally
            {
                ResilienceContextPool.Shared.Return(context);
            }
        }
        catch (HttpRequestException ex)
        {
            Logs.HttpRequestFailed(Logger, endpoint, ex);
            throw;
        }
    }

    /// <summary>
    /// Core scaffolding with JSON deserialization: delegates to ExecuteAsync, reads and
    /// deserializes the response body. Also catches and logs JsonException.
    /// </summary>
    private async Task<T> ExecuteAsync<T>(
        string endpoint,
        Func<ResilienceContext, Task<HttpResponseMessage>> httpFactory,
        CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            var response = await ExecuteAsync(endpoint, httpFactory, cancellationToken)
                .ConfigureAwait(false);

            var result = await response.Content
                .ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (result == null)
            {
                Logs.ApiResponseDataNull(Logger, endpoint);
                throw new InvalidOperationException($"API response data is null for endpoint: {endpoint}");
            }

            return result;
        }
        catch (JsonException ex)
        {
            Logs.DeserializationFailed(Logger, endpoint, ex);
            throw;
        }
    }

    private static partial class Logs
    {
        [LoggerMessage(EventId = 10, Level = LogLevel.Warning, Message = "API response data is null for endpoint: {Endpoint}")]
        internal static partial void ApiResponseDataNull(ILogger logger, string endpoint);

        [LoggerMessage(EventId = 11, Level = LogLevel.Error, Message = "HTTP request failed for endpoint: {Endpoint}")]
        internal static partial void HttpRequestFailed(ILogger logger, string endpoint, Exception ex);

        [LoggerMessage(EventId = 12, Level = LogLevel.Error, Message = "Failed to deserialize response for endpoint: {Endpoint}")]
        internal static partial void DeserializationFailed(ILogger logger, string endpoint, Exception ex);
    }
}
