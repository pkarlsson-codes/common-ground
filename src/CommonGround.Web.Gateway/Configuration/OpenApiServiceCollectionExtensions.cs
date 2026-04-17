using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace CommonGround.Web.Gateway.Configuration;

/// <summary>
/// OpenAPI/NSwag registration helpers.
/// </summary>
public static class OpenApiServiceCollectionExtensions
{
    /// <summary>
    /// Registers OpenAPI document and explorer with development servers.
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <param name="configuration">Application configuration, used to populate server URLs.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApiDocument(options =>
        {
            options.DocumentName = "v1";
            options.Title = "CommonGround.Web.Gateway";
            options.Version = "1.0";
            options.Description = "CommonGround Web Gateway API";
            options.OperationProcessors.Add(new EndpointNameOperationProcessor());
            options.PostProcess = document =>
            {
                var httpUrl = configuration["Gateway:BaseUrl"]
                    ?? "http://localhost:4000";
                document.Servers.Clear();
                document.Servers.Add(new NSwag.OpenApiServer { Url = httpUrl, Description = "HTTP dev (default)" });
            };
        });
        return services;
    }

    /// <summary>
    /// NSwag operation processor that uses <see cref="EndpointNameAttribute"/> as the operation ID.
    /// </summary>
    private sealed class EndpointNameOperationProcessor : IOperationProcessor
    {
        /// <inheritdoc/>
        public bool Process(OperationProcessorContext context)
        {
            var endpointName = context.MethodInfo
                .GetCustomAttributes(typeof(EndpointNameAttribute), inherit: false)
                .OfType<EndpointNameAttribute>()
                .FirstOrDefault();

            if (endpointName is not null)
            {
                context.OperationDescription.Operation.OperationId = endpointName.EndpointName;
            }

            return true;
        }
    }
}
