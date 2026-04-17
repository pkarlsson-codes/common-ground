using CommonGround.SharedKernel.Infrastructure.API;
using Scalar.AspNetCore;

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

        app.UseExceptionHandler();
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
        app.MapControllers();

        return app;
    }
}