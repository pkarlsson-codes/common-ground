using CommonGround.SharedKernel.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Persistence;

namespace CommonGround.API.Infrastructure.ExceptionHandling;

/// <summary>
/// Handles <see cref="AggregateNotFoundException"/>, and Wolverine's <see cref="RequiredDataMissingException"/>
/// for an aggregate handler whose stream doesn't exist, and writes a 404 Not Found ProblemDetails response.
/// </summary>
internal sealed class AggregateNotFoundExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    /// <inheritdoc/>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not AggregateNotFoundException && exception is not RequiredDataMissingException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "Resource not found",
                Detail = exception.Message,
                Status = StatusCodes.Status404NotFound,
            },
        }).ConfigureAwait(false);
    }
}