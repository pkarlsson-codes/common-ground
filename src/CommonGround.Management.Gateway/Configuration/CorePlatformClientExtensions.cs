using CommonGround.API.Clients;
using CommonGround.Management.Gateway.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace CommonGround.Management.Gateway.Configuration;

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

        var corePlatformUri = new Uri(corePlatformBaseAddress);

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

        services.AddApiClient<ILandParcelsApiClient, LandParcelsApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IHomeownerApplicationsApiClient, HomeownerApplicationsApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IHomeSelectionsApiClient, HomeSelectionsApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IHouseholdsApiClient, HouseholdsApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IHomesApiClient, HomesApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IGroundLeasesApiClient, GroundLeasesApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IResalesApiClient, ResalesApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IPurchaseWorkflowsApiClient, PurchaseWorkflowsApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IUsersApiClient, UsersApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalServiceTokenHandler>();

        services.AddApiClient<ITrustsApiClient, TrustsApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IDemoApiClient, DemoApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IStewardshipApiClient, StewardshipApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        // Downloads hand the API's redirect to the presigned S3 URL back to the browser, so don't follow it.
        services.AddApiClient<IDocumentsApiClient, DocumentsApiClient>(corePlatformUri)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false })
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IDevelopmentProjectsApiClient, DevelopmentProjectsApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IGroundLeaseTemplatesApiClient, GroundLeaseTemplatesApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<ITodayApiClient, TodayApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IRemindersApiClient, RemindersApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IIncomeLimitSchedulesApiClient, IncomeLimitSchedulesApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IPortfolioImportApiClient, PortfolioImportApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        services.AddApiClient<IReportsApiClient, ReportsApiClient>(corePlatformUri)
            .AddHttpMessageHandler<InternalGatewayAuthHandler>();

        return services;
    }
}
