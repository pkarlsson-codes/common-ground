using CommonGround.API.Clients;
using CommonGround.Web.Gateway.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace CommonGround.Web.Gateway.Configuration;

/// <summary>
/// Extension methods for registering Core Platform API clients in the Web Gateway.
/// </summary>
internal static class CorePlatformClientExtensions
{
    /// <summary>
    /// Adds Core Platform API clients to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when CorePlatform:BaseAddress configuration is missing.</exception>
    public static IServiceCollection AddCorePlatformClients(this IServiceCollection services, ConfigurationManager configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var corePlatformBaseAddress = configuration["CorePlatform:BaseAddress"]
            ?? throw new InvalidOperationException("CorePlatform:BaseAddress configuration is required");

        services.AddHttpContextAccessor();
        services.AddSingleton<IInternalJwtService, InternalJwtService>();
        services.TryAddScoped<InternalGatewayAuthHandler>();
        services.AddTransient<InternalServiceTokenHandler>();

        services.AddHttpClient(HttpClientNames.CorePlatformSystem, (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<CorePlatformOptions>>();
            client.BaseAddress = new Uri(options.Value.BaseAddress ?? corePlatformBaseAddress);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }).AddHttpMessageHandler<InternalServiceTokenHandler>();

        services.AddHttpClient<ILandParcelsApiClient, LandParcelsApiClient>(client =>
            client.BaseAddress = new Uri(corePlatformBaseAddress))
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddHttpClient<IHomesApiClient, HomesApiClient>(client =>
            client.BaseAddress = new Uri(corePlatformBaseAddress))
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddHttpClient<IGroundLeasesApiClient, GroundLeasesApiClient>(client =>
            client.BaseAddress = new Uri(corePlatformBaseAddress))
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddHttpClient<IPurchaseWorkflowsApiClient, PurchaseWorkflowsApiClient>(client =>
            client.BaseAddress = new Uri(corePlatformBaseAddress))
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddHttpClient<IUsersApiClient, UsersApiClient>(client =>
            client.BaseAddress = new Uri(corePlatformBaseAddress))
            .AddHttpMessageHandler<InternalServiceTokenHandler>();

        services.AddHttpClient<IStewardshipApiClient, StewardshipApiClient>(client =>
            client.BaseAddress = new Uri(corePlatformBaseAddress))
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddHttpClient<IDocumentsApiClient, DocumentsApiClient>(client =>
            client.BaseAddress = new Uri(corePlatformBaseAddress))
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddHttpClient<IDevelopmentProjectsApiClient, DevelopmentProjectsApiClient>(client =>
            client.BaseAddress = new Uri(corePlatformBaseAddress))
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        return services;
    }
}
