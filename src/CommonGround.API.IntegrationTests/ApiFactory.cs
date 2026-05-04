using CommonGround.API.IntegrationTests.Documents;
using CommonGround.Documents.Domain.Documents;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Wolverine;

namespace CommonGround.API.IntegrationTests;

/// <summary>
/// Wolverine extension that switches durability off for integration tests.
/// </summary>
internal sealed class MediatorOnlyExtension : IWolverineExtension
{
    /// <inheritdoc />
    public void Configure(WolverineOptions options)
    {
        options.Durability.Mode = DurabilityMode.MediatorOnly;
    }
}

/// <summary>
/// Boots the CommonGround API in-process for integration tests.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    /// <summary>The JWT issuer expected by the API under the Testing environment.</summary>
    internal const string JwtIssuer = "CommonGround.Gateway";

    /// <summary>The JWT audience expected by the API under the Testing environment.</summary>
    internal const string JwtAudience = "CommonGround.API";

    /// <summary>The symmetric key used to sign test JWTs.</summary>
    internal const string JwtSigningKey = "integration-test-signing-key-must-be-32ch";

    /// <summary>The trust identifier used by default in test JWTs.</summary>
    internal const string TestTrustId = "trust-testdefault00000000000";

    private readonly PostgreSqlContainer _pgContainer = BuildPostgresContainer();
    private int _containerStarted;

    private static PostgreSqlContainer BuildPostgresContainer()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Testing.json", optional: true)
            .Build();

        return new PostgreSqlBuilder()
            .WithDatabase(config["TestDatabase:Database"] ?? "commonground_test")
            .WithUsername(config["TestDatabase:Username"] ?? "postgres")
            .WithPassword(config["TestDatabase:Password"] ?? "postgres")
            .Build();
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        try
        {
            await base.DisposeAsync().ConfigureAwait(false);
        }
        catch
        {
            // Suppress teardown failures caused by hosted background services.
        }

        await _pgContainer.DisposeAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        EnsureContainerStarted();

        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _pgContainer.GetConnectionString(),
                ["InternalJwt:Issuer"] = JwtIssuer,
                ["InternalJwt:Audience"] = JwtAudience,
                ["InternalJwt:SigningKey"] = JwtSigningKey,
            };
            configurationBuilder.AddInMemoryCollection(overrides);
        });

        builder.ConfigureServices(services =>
        {
            services.ConfigureMarten(options =>
            {
                options.Connection(_pgContainer.GetConnectionString());

                foreach (var source in options.Projections.All.OfType<ProjectionBase>())
                {
                    source.Lifecycle = ProjectionLifecycle.Inline;
                }
            });

            services.AddSingleton<IDocumentBlobStore, InMemoryDocumentBlobStore>();

            services.AddWolverineExtension<MediatorOnlyExtension>();
        });
    }

    private void EnsureContainerStarted()
    {
        if (Interlocked.Exchange(ref _containerStarted, 1) == 1)
        {
            return;
        }

        _pgContainer.StartAsync().GetAwaiter().GetResult();
    }
}
