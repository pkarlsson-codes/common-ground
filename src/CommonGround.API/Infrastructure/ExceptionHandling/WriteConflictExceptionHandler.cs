using JasperFx;
using JasperFx.Events;
using Marten.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace CommonGround.API.Infrastructure.ExceptionHandling;

/// <summary>
/// Handles a write that lost a race to a concurrent one, such as two home selections started for
/// the same home at once, and writes a 409 Conflict ProblemDetails response. Domain checks catch
/// these cases first; a unique index catches what slips past them, and Marten reports it as
/// <see cref="DocumentAlreadyExistsException"/> or a raw unique violation. Appends that keep losing
/// to concurrent ones on the same event stream, after Wolverine's retries, are conflicts too.
/// </summary>
internal sealed class WriteConflictExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    private const string ConflictMessage = "Someone else made a conflicting change at the same time. Refresh and try again.";

    /// <inheritdoc/>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (!IsWriteConflict(exception))
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "Conflicting change",
                Detail = ConflictMessage,
                Status = StatusCodes.Status409Conflict,
            },
        }).ConfigureAwait(false);
    }

    private static bool IsWriteConflict(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is DocumentAlreadyExistsException
                || current is EventStreamUnexpectedMaxEventIdException
                || current is ConcurrencyException
                || current is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                return true;
            }
        }

        return false;
    }
}
