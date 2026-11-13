using Marten;

namespace CommonGround.API.Demo;

/// <summary>Registers demo mode.</summary>
internal static class DemoServiceCollectionExtensions
{
    /// <summary>Registers <see cref="DemoOptions"/>, checked at startup, and the <see cref="DemoSandbox"/> document.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDemo(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DemoOptions>()
            .Bind(configuration.GetSection(DemoOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<Microsoft.Extensions.Options.IValidateOptions<DemoOptions>, DemoOptionsValidator>();

        services.ConfigureMarten(options => options.Schema.For<DemoSandbox>().DatabaseSchemaName("demo"));

        return services;
    }
}
