using CommonGround.API;
using CommonGround.API.Infrastructure;
using CommonGround.API.Infrastructure.Configuration;
using CommonGround.API.Infrastructure.Messaging;
using CommonGround.API.Infrastructure.Seeding;
using CommonGround.Documents.Infrastructure;
using CommonGround.HomeBuyers.Infrastructure;
using CommonGround.Homeownership.Infrastructure;
using CommonGround.Identity.Infrastructure;
using CommonGround.PropertyRegistry.Infrastructure;
using CommonGround.Stewardship;
using CommonGround.TrustManagement.Infrastructure;
using JasperFx;
using JasperFx.Core;
using JasperFx.Events;
using Marten.Exceptions;
using Serilog;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.Http;
using Wolverine.Persistence;
using Wolverine.Postgresql.Transport;
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

    var seeding = SeedCommand.IsRequested(args);
    if (seeding)
    {
        SeedCommand.Configure(builder);
    }

    var messaging = builder.Configuration.GetSection(MessagingOptions.SectionName).Get<MessagingOptions>()
        ?? new MessagingOptions();

    System.Reflection.Assembly[] handlerAssemblies =
    [
        typeof(CommonGroundApiAssemblyReference).Assembly,
        typeof(HomeBuyersAssemblyReference).Assembly,
        typeof(PropertyRegistryAssemblyReference).Assembly,
        typeof(HomeownershipAssemblyReference).Assembly,
        typeof(IdentityAssemblyReference).Assembly,
        typeof(StewardshipAssemblyReference).Assembly,
        typeof(DocumentsAssemblyReference).Assembly,
        typeof(TrustManagementAssemblyReference).Assembly,
    ];

    builder.Host.UseWolverine(opts =>
    {
        opts.Policies.AutoApplyTransactions();

        // Local queues go through the Marten inbox, so a message survives a restart and a scheduled one
        // (a prequalification's expiry, months out) is stored and polled for rather than held in an
        // in-memory Task.Delay, which can't wait longer than ~24.8 days.
        opts.Policies.UseDurableLocalQueues();

        // A message several bounded contexts react to (a household selected, a purchase closed or fallen
        // through, a home registered) gets one handler per context, each on its own local queue with its
        // own transaction and retries, so one context's failure doesn't roll back or re-run another's.
        opts.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;

        // A [WriteAggregate] handler whose stream doesn't exist must fail, not silently skip.
        // The exception handler turns RequiredDataMissingException into a 404.
        opts.EntityDefaults.OnMissing = OnMissing.ThrowException;

        // Two handlers writing the same event stream or versioned document at once (e.g. news of a home's
        // selection opening and of its household being chosen) lose the race with a version conflict, or,
        // for a document both are creating, a duplicate. Re-running the handler reloads what it writes, so
        // the loser applies its change on top of the winner's.
        opts.OnException<EventStreamUnexpectedMaxEventIdException>()
            .Or<ConcurrencyException>()
            .Or<DocumentAlreadyExistsException>()
            .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());

        switch (messaging.Transport)
        {
            case MessageTransport.RabbitMq:
                var rabbitMq = messaging.RabbitMq;
                opts.UseRabbitMq(rabbit =>
                {
                    rabbit.HostName = rabbitMq.Host;
                    rabbit.Port = rabbitMq.Port;
                    rabbit.UserName = rabbitMq.Username;
                    rabbit.Password = rabbitMq.Password;
                }).AutoProvision().UseConventionalRouting(routing =>
                {
                    routing.IncludeTypes(IntegrationEvents.Includes);
                    routing.OnlyApplyToOutboundMessages();
                });
                break;

            case MessageTransport.Postgres:
                // The queue table lives in the database Marten already uses (IntegrateWithWolverine).
                var queue = opts.Transports.GetOrCreate<PostgresqlTransport>().Queues[messaging.PostgresQueue];
                opts.RouteWith(new PostgresIntegrationEventRouting(queue));
                break;
        }

        // An event sent to an external transport still reaches this process's own handlers.
        opts.Policies.ConventionalLocalRoutingIsAdditive();

        foreach (var assembly in handlerAssemblies)
        {
            opts.Discovery.IncludeAssembly(assembly);
        }
    });

    var app = builder.Build();

    if (seeding)
    {
        return await SeedCommand.RunAsync(app).ConfigureAwait(false);
    }

    app.ConfigureDevelopmentPipeline();
    app.ConfigureStandardPipeline();
    app.ConfigureEndpoints();

    // A JasperFx command such as `codegen test`, `describe` or `resources` (WebApplicationFactory and
    // the plain run pass only `--option` arguments).
    if (args.Length > 0 && !args[0].StartsWith('-'))
    {
        return await app.RunJasperFxCommands(args).ConfigureAwait(false);
    }

    await app.RunAsync().ConfigureAwait(false);
    return 0;
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
