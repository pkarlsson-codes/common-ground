using CommonGround.API;
using CommonGround.API.Infrastructure;
using CommonGround.API.Infrastructure.Configuration;
using CommonGround.API.Infrastructure.Seeding;
using CommonGround.Documents.Infrastructure;
using CommonGround.HomeBuyers.Infrastructure;
using CommonGround.Homeownership.Infrastructure;
using CommonGround.Identity.Infrastructure;
using CommonGround.PropertyRegistry.Infrastructure;
using CommonGround.Stewardship;
using CommonGround.TrustManagement.Infrastructure;
using Serilog;
using Wolverine;
using Wolverine.Http;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting CommonGround API");

    builder.Services.AddConfiguredControllers();
    builder.Services.AddConfiguredSwagger();
    builder.Services.AddConfiguredCors(builder.Configuration);
    builder.Services.AddCommonGroundServices(builder.Configuration, builder.Environment);
    builder.Services.AddWolverineHttp();

    var rabbitMqOptions = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>() ?? new RabbitMqOptions();
    var useRabbitMq = !builder.Environment.IsEnvironment("Testing");

    builder.Host.UseWolverine(opts =>
    {
        opts.Policies.AutoApplyTransactions();

        if (useRabbitMq)
        {
            opts.UseRabbitMq(rabbit =>
            {
                rabbit.HostName = rabbitMqOptions.Host;
                rabbit.Port = rabbitMqOptions.Port;
                rabbit.UserName = rabbitMqOptions.Username;
                rabbit.Password = rabbitMqOptions.Password;
            }).AutoProvision().UseConventionalRouting(routing =>
            {
                routing.IncludeTypes(t =>
                    t.Namespace?.EndsWith(".Events", StringComparison.Ordinal) == true &&
                    t.Assembly.GetName().Name?.EndsWith(".Contracts", StringComparison.Ordinal) == true);
                routing.OnlyApplyToOutboundMessages();
            });

            opts.Policies.ConventionalLocalRoutingIsAdditive();
        }

        opts.Discovery.IncludeAssembly(typeof(CommonGroundApiAssemblyReference).Assembly);
        opts.Discovery.IncludeAssembly(typeof(ApplicantSelectionAssemblyReference).Assembly);
        opts.Discovery.IncludeAssembly(typeof(PropertyRegistryAssemblyReference).Assembly);
        opts.Discovery.IncludeAssembly(typeof(HomeownershipAssemblyReference).Assembly);
        opts.Discovery.IncludeAssembly(typeof(IdentityAssemblyReference).Assembly);
        opts.Discovery.IncludeAssembly(typeof(StewardshipAssemblyReference).Assembly);
        opts.Discovery.IncludeAssembly(typeof(DocumentsAssemblyReference).Assembly);
        opts.Discovery.IncludeAssembly(typeof(TrustManagementAssemblyReference).Assembly);
    });

    var app = builder.Build();

    app.ConfigureDevelopmentPipeline();
    app.ConfigureStandardPipeline();
    app.ConfigureEndpoints();

    await app.StartAsync().ConfigureAwait(false);
    await DevelopmentSeeder.RunAsync(app).ConfigureAwait(false);
    await app.WaitForShutdownAsync().ConfigureAwait(false);
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>Entry point - exposed as partial to allow WebApplicationFactory access in integration tests.</summary>
public partial class Program { }
