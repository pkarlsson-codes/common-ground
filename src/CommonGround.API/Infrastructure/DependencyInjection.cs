using System.Text;
using CommonGround.API.Infrastructure.ExceptionHandling;
using CommonGround.API.Infrastructure.Seeding;
using CommonGround.ApplicantSelection.Infrastructure;
using CommonGround.ApplicantSelection.Infrastructure.Persistence;
using CommonGround.Documents.Infrastructure;
using CommonGround.Documents.Infrastructure.Persistence;
using CommonGround.Homeownership.Infrastructure;
using CommonGround.Homeownership.Infrastructure.Persistence;
using CommonGround.Identity.Infrastructure;
using CommonGround.Identity.Infrastructure.Persistence;
using CommonGround.PropertyRegistry.Infrastructure;
using CommonGround.PropertyRegistry.Infrastructure.Persistence;
using CommonGround.PropertyRegistry.Infrastructure.Seeding;
using CommonGround.SharedKernel.Extensions;
using CommonGround.Stewardship.Infrastructure;
using CommonGround.Stewardship.Infrastructure.Persistence;
using CommonGround.TrustManagement.Infrastructure;
using CommonGround.TrustManagement.Infrastructure.Persistence;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

namespace CommonGround.API.Infrastructure;

/// <summary>Dependency injection configuration for the CommonGround API.</summary>
internal static class CommonGroundServiceCollectionExtensions
{
    /// <summary>Registers all CommonGround API services.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="environment">The host environment.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCommonGroundServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddCommonGroundCore();
        services.AddCommonGroundExceptionHandling();
        services.AddCommonGroundDatabase(configuration);
        services.AddCommonGroundHealthChecks();
        services.AddCommonGroundAuthentication(configuration, environment);
        services.AddCommonGroundAuthorization();
        services.AddApplicantSelection();
        services.AddPropertyRegistry();
        services.AddHomeownership();
        services.AddIdentity();
        services.AddTrustManagement();
        services.AddStewardship();
        services.AddDocuments(configuration, environment);
        return services;
    }

    private static IServiceCollection AddCommonGroundExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<AggregateNotFoundExceptionHandler>();
        services.AddExceptionHandler<ForbiddenExceptionHandler>();
        services.AddExceptionHandler<DomainExceptionHandler>();
        services.AddExceptionHandler<UnhandledExceptionHandler>();
        return services;
    }

    private static IServiceCollection AddCommonGroundCore(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);
        services.AddFluentValidationAutoValidation();
        return services;
    }

    private static IServiceCollection AddCommonGroundDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Database connection string not found");

        services.AddMartenServices(connectionString, opts =>
        {
            ApplicantSelectionMartenConfiguration.Configure(opts);
            PropertyRegistryMartenConfiguration.Configure(opts);
            HomeownershipMartenConfiguration.Configure(opts);
            IdentityMartenConfiguration.Configure(opts);
            TrustManagementMartenConfiguration.Configure(opts);
            StewardshipMartenConfiguration.Configure(opts);
            DocumentsMartenConfiguration.Configure(opts);
        }, configureBuilder: marten =>
        {
            marten.InitializeWith<TrustsInitialData>();
            marten.InitializeWith<LandParcelsInitialData>();
            marten.InitializeWith<HomesInitialData>();
            marten.InitializeWith<UsersInitialData>();
        });

        return services;
    }

    private static IServiceCollection AddCommonGroundHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());
        return services;
    }

    private static IServiceCollection AddCommonGroundAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var issuer = configuration["InternalJwt:Issuer"]
            ?? throw new InvalidOperationException("InternalJwt:Issuer is not configured.");
        var audience = configuration["InternalJwt:Audience"]
            ?? throw new InvalidOperationException("InternalJwt:Audience is not configured.");
        var secret = configuration["InternalJwt:SigningKey"]
            ?? Environment.GetEnvironmentVariable("COMMONGROUND_INTERNAL_JWT_SECRET")
            ?? throw new InvalidOperationException(
                "Internal JWT signing key not configured. Set InternalJwt:SigningKey or COMMONGROUND_INTERNAL_JWT_SECRET.");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "user_id",
                };
            });

        return services;
    }

    private static IServiceCollection AddCommonGroundAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
        });
        return services;
    }
}