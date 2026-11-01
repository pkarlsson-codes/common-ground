using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.API.Clients;

/// <summary>Registers API clients so a gateway's handlers can take them as plain constructor dependencies.</summary>
public static class ApiClientServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TImplementation"/> as a transient <typeparamref name="TClient"/>, and the named
    /// HTTP client it creates (named after the implementation type) pointing at <paramref name="baseAddress"/>.
    /// A typed <c>AddHttpClient&lt;TClient, TImplementation&gt;</c> registration is an opaque factory that
    /// Wolverine's code generation can't build inline, so it refuses to compile handlers that depend on it.
    /// </summary>
    /// <typeparam name="TClient">The client interface.</typeparam>
    /// <typeparam name="TImplementation">The client implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="baseAddress">The API's base address.</param>
    /// <returns>The named HTTP client's builder, for adding message handlers.</returns>
    public static IHttpClientBuilder AddApiClient<TClient, TImplementation>(this IServiceCollection services, Uri baseAddress)
        where TClient : class
        where TImplementation : class, TClient
    {
        services.AddTransient<TClient, TImplementation>();
        return services.AddHttpClient(typeof(TImplementation).Name, client => client.BaseAddress = baseAddress);
    }
}
