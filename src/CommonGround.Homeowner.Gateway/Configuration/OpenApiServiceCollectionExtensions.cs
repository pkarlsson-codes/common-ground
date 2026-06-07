using Microsoft.AspNetCore.Mvc;
using Namotion.Reflection;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace CommonGround.Homeowner.Gateway.Configuration;

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
            options.Title = "CommonGround.Homeowner.Gateway";
            options.Version = "1.0";
            options.Description = "CommonGround Homeowner Gateway API";
            options.OperationProcessors.Add(new EndpointNameOperationProcessor());
            options.OperationProcessors.Add(new ProblemDetailsResponseProcessor());
            options.PostProcess = document =>
            {
                var httpUrl = configuration["Gateway:BaseUrl"]
                    ?? "http://localhost:4001";
                document.Servers.Clear();
                document.Servers.Add(new NSwag.OpenApiServer { Url = httpUrl, Description = "HTTP dev (default)" });
            };
        });
        return services;
    }

    /// <summary>
    /// NSwag operation processor that uses <see cref="EndpointNameAttribute"/> as the operation ID
    /// and derives OpenAPI tags from the declaring class name so the TypeScript generator
    /// can group operations into named API classes.
    /// </summary>
    private sealed class ProblemDetailsResponseProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            var schema = context.SchemaGenerator.GenerateWithReferenceAndNullability<JsonSchema>(
                typeof(ProblemDetails).ToContextualType(), context.SchemaResolver);
            var response = new OpenApiResponse
            {
                Description = "Error",
                Content = { ["application/problem+json"] = new OpenApiMediaType { Schema = schema } },
            };
            context.OperationDescription.Operation.Responses["400"] = response;
            return true;
        }
    }

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
            else
            {
                context.OperationDescription.Operation.OperationId = context.MethodInfo.Name;
            }

            if (context.OperationDescription.Operation.Tags.Count == 0 ||
                context.OperationDescription.Operation.Tags.All(t => t is null))
            {
                var declaringType = context.MethodInfo.DeclaringType;
                if (declaringType is not null)
                {
                    var tag = declaringType.Name.EndsWith("Endpoints", StringComparison.Ordinal)
                        ? declaringType.Name[..^"Endpoints".Length]
                        : declaringType.Name;
                    context.OperationDescription.Operation.Tags.Clear();
                    context.OperationDescription.Operation.Tags.Add(tag);
                }
            }

            return true;
        }
    }
}
