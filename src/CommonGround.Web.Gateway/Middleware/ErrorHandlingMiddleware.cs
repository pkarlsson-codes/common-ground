using System.Net;
using System.Text.Json;
using CommonGround.SharedKernel.Infrastructure.API;

namespace CommonGround.Web.Gateway.Middleware;

/// <summary>
/// Middleware for handling exceptions and providing consistent error responses.
/// </summary>
internal sealed partial class ErrorHandlingMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware with the given HTTP context.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex).ConfigureAwait(false);
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    private static async Task<bool> TryForwardUpstreamResponse(HttpResponse response, Exception exception)
    {
        if (exception is not UpstreamApiException { ResponseBody.Length: > 0 } upstream)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(upstream.ContentType))
        {
            response.ContentType = upstream.ContentType;
        }

        await response.WriteAsync(upstream.ResponseBody).ConfigureAwait(false);
        return true;
    }

    private static string GetMessageForStatus(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.NotFound => "The requested resource was not found",
        HttpStatusCode.Unauthorized => "Access denied",
        HttpStatusCode.Forbidden => "Access forbidden",
        HttpStatusCode.UnprocessableEntity => "The request could not be processed",
        HttpStatusCode.BadRequest => "Invalid request",
        HttpStatusCode.Conflict => "A conflict occurred",
        _ when (int)statusCode >= 500 => "An upstream service error occurred",
        _ => "The request could not be completed",
    };

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.TraceIdentifier;

        Logs.LogUnhandledException(_logger, correlationId, exception);

        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, errorMessage) = exception switch
        {
            UpstreamApiException upstreamFailure when upstreamFailure.StatusCode.HasValue => (upstreamFailure.StatusCode.Value, GetMessageForStatus(upstreamFailure.StatusCode.Value)),
            HttpRequestException httpEx when httpEx.StatusCode.HasValue => (httpEx.StatusCode.Value, GetMessageForStatus(httpEx.StatusCode.Value)),
            HttpRequestException => (HttpStatusCode.BadGateway, "An upstream service error occurred"),
            ArgumentNullException => (HttpStatusCode.BadRequest, "Required parameter is missing"),
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid request parameters"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Access denied"),
            InvalidOperationException => (HttpStatusCode.BadRequest, "Invalid operation"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        response.StatusCode = (int)statusCode;

        if (await TryForwardUpstreamResponse(response, exception).ConfigureAwait(false))
        {
            return;
        }

        var errorResponse = new
        {
            error = errorMessage,
            requestId = correlationId,
            timestamp = DateTimeOffset.UtcNow,
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, SerializerOptions);

        await response.WriteAsync(jsonResponse).ConfigureAwait(false);
    }

    private static partial class Logs
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Unhandled exception occurred. Correlation ID: {CorrelationId}")]
        internal static partial void LogUnhandledException(ILogger logger, string correlationId, Exception? exception);
    }
}

/// <summary>
/// Extension methods for registering the error handling middleware.
/// </summary>
internal static class ErrorHandlingMiddlewareExtensions
{
    /// <summary>
    /// Registers the global error handling middleware in the ASP.NET Core pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
