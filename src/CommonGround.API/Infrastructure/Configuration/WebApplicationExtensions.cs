using CommonGround.SharedKernel.Infrastructure.API;
using Scalar.AspNetCore;
using Serilog;
using Wolverine.Http;

namespace CommonGround.API.Infrastructure.Configuration;

/// <summary>
/// Extension methods for configuring the WebApplication pipeline.
/// </summary>
internal static class WebApplicationExtensions
{
    /// <summary>
    /// Configures development-only middleware.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication ConfigureDevelopmentPipeline(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.MapOpenApi();
        app.MapScalarApiReference();

        return app;
    }

    /// <summary>
    /// Configures standard middleware for all environments.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication ConfigureStandardPipeline(this WebApplication app)
    {
        // Request logging wraps the exception handler, so it logs the status the handler wrote
        // (e.g. 404 for AggregateNotFoundException) rather than a 500 for the escaping exception.
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });
        app.UseExceptionHandler();
        app.UseCorrelationId();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    /// <summary>
    /// Configures endpoint mapping.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication ConfigureEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health").AllowAnonymous();
        app.MapGet("/api/test-auth", () => Results.Ok(new { success = true }))
            .RequireAuthorization();

        app.MapWolverineEndpoints(opts =>
        {
            opts.TenantId.IsClaimTypeNamed("tenant_id");
            opts.TenantId.AssertExists();
        });

        return app;
    }
}