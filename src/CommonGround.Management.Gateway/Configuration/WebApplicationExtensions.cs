using CommonGround.Management.Gateway.Middleware;
using Wolverine.Http;

namespace CommonGround.Management.Gateway.Configuration;

/// <summary>
/// Extension methods for configuring the WebApplication middleware pipeline.
/// </summary>
internal static partial class WebApplicationExtensions
{
    /// <summary>
    /// Configures the request pipeline for development environments.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication ConfigureDevelopmentPipeline(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.UseOpenApi();
        app.UseSwaggerUi();

        return app;
    }

    /// <summary>
    /// Configures security-related middleware including global error handling, CORS,
    /// security headers, CSRF protection, and authentication/authorization.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication ConfigureSecurityPipeline(this WebApplication app)
    {
        app.UseGlobalErrorHandling();
        app.UseCors();
        app.UseSecurityHeaders();
        app.UseAuthentication();
        app.UseCsrfProtection();
        app.UseAuthorization();

        return app;
    }

    /// <summary>
    /// Configures API-specific middleware and endpoint mapping.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication ConfigureApiPipeline(this WebApplication app)
    {
        app.MapHealthChecks("/health").AllowAnonymous();
        app.MapControllers();
        app.MapWolverineEndpoints();

        return app;
    }

    /// <summary>
    /// Validates that all required services and configuration are available.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required services or configuration are missing.</exception>
    public static WebApplication ValidateStartupRequirements(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebApplication>>();

        try
        {
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var requiredSections = new[]
            {
                "CorePlatform:BaseAddress",
                "Authentication:OIDC:Authority",
                "Authentication:OIDC:ClientId",
            };

            foreach (var section in requiredSections)
            {
                var value = configuration[section];
                if (string.IsNullOrWhiteSpace(value))
                {
                    Logs.LogConfigurationMissing(logger, section);
                }
            }

            Logs.LogStartupValidationCompleted(logger);
        }
        catch (Exception ex)
        {
            Logs.LogStartupValidationFailed(logger, ex);
            throw;
        }

        return app;
    }

    private static partial class Logs
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Startup validation failed")]
        internal static partial void LogStartupValidationFailed(ILogger logger, Exception? exception);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Startup validation completed successfully")]
        internal static partial void LogStartupValidationCompleted(ILogger logger);

        [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Configuration section '{Section}' is missing or empty")]
        internal static partial void LogConfigurationMissing(ILogger logger, string section);
    }
}
