using CommonGround.Management.Gateway.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace CommonGround.Management.Gateway.Configuration;

/// <summary>
/// Registers session storage and the Data Protection key ring in Postgres.
/// </summary>
public static class TokenStoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers a Postgres <see cref="ITokenStore"/> and Data Protection key repository using
    /// the connection string <c>ConnectionStrings:Database</c>.
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the connection string is missing.</exception>
    public static IServiceCollection AddTokenStoreFromConfig(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("A Postgres connection string is required for the session store (ConnectionStrings:Database)");
        }

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(new GatewayDatabase(NpgsqlDataSource.Create(connectionString)));
        services.AddSingleton<PostgresTokenStore>();
        services.AddSingleton<ITokenStore>(sp => sp.GetRequiredService<PostgresTokenStore>());

        // First of all hosted services: Data Protection's own hosted service reads the key ring at startup,
        // and on a new database the table it reads doesn't exist until this one has run.
        services.Insert(0, ServiceDescriptor.Singleton<IHostedService, GatewayDatabaseMaintenance>());

        services.AddDataProtection().SetApplicationName("commonground-management-gateway");
        services.AddOptions<KeyManagementOptions>()
            .Configure<GatewayDatabase>((options, database) => options.XmlRepository = new PostgresXmlRepository(database));

        return services;
    }
}
