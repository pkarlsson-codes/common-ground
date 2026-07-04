using System.Text.RegularExpressions;
using CommonGround.SharedKernel.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.API.Infrastructure.ExceptionHandling;

/// <summary>
/// Handles <see cref="DomainException"/> (including <see cref="BusinessRuleViolationException"/>
/// and <see cref="InvariantViolationException"/>) and writes a 422 Unprocessable Entity ProblemDetails response.
/// </summary>
internal sealed class DomainExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    private const string GenericErrorMessage = "An error occurred while processing your request. Please try again or contact support.";

    /// <inheritdoc/>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (UnwrapDomainException(exception) is not DomainException domainException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

        var userFacingMessage = GetUserFacingMessage(domainException);

        var problemDetails = new ProblemDetails
        {
            Title = "Business rule violation",
            Detail = userFacingMessage,
            Status = StatusCodes.Status422UnprocessableEntity,
        };

        // Include error code if available for client-side error handling
        if (!string.IsNullOrWhiteSpace(domainException.Code))
        {
            problemDetails.Extensions["code"] = domainException.Code;
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails,
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Walks the exception chain to find a <see cref="DomainException"/>. Inline Marten
    /// projections (e.g. concurrency guards that throw from an Apply method) surface their
    /// exceptions wrapped in a JasperFx <c>ApplyEventException</c>, so a direct type check
    /// on the top-level exception is not sufficient.
    /// </summary>
    private static DomainException? UnwrapDomainException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is DomainException domainException)
            {
                return domainException;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a user-facing error message by looking up the error code in the message map
    /// and substituting any template parameters from the exception's Args.
    /// Falls back to a generic message if the code is not found.
    /// </summary>
    private static string GetUserFacingMessage(DomainException exception)
    {
        if (string.IsNullOrWhiteSpace(exception.Code))
        {
            return GenericErrorMessage;
        }

        if (!BusinessRuleErrorMessageMap.ByCode.TryGetValue(exception.Code, out var template))
        {
            return GenericErrorMessage;
        }

        return SubstituteParameters(template, exception.Args);
    }

    /// <summary>
    /// Substitutes template parameters in the format {paramName} with values from the args dictionary.
    /// </summary>
    private static string SubstituteParameters(string template, IReadOnlyDictionary<string, object?> args)
    {
        if (args.Count == 0)
        {
            return template;
        }

        var result = template;
        foreach (var kvp in args)
        {
            var placeholder = $"{{{kvp.Key}}}";
            var value = kvp.Value?.ToString() ?? string.Empty;
            result = Regex.Replace(result, Regex.Escape(placeholder), value, RegexOptions.IgnoreCase);
        }

        return result;
    }
}