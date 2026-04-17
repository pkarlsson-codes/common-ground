using CommonGround.API;
using CommonGround.API.Infrastructure;
using CommonGround.API.Infrastructure.Configuration;
using Serilog;
using Wolverine;
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

    var rabbitMqOptions = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>() ?? new RabbitMqOptions();
    var useRabbitMq = !builder.Environment.IsEnvironment("Testing");

    builder.Host.UseWolverine(opts =>
    {
        if (useRabbitMq)
        {
            opts.UseRabbitMq(rabbit =>
            {
                rabbit.HostName = rabbitMqOptions.Host;
                rabbit.Port = rabbitMqOptions.Port;
                rabbit.UserName = rabbitMqOptions.Username;
                rabbit.Password = rabbitMqOptions.Password;
            }).AutoProvision();

            opts.Policies.ConventionalLocalRoutingIsAdditive();
        }

        opts.Discovery.IncludeAssembly(typeof(CommonGroundApiAssemblyReference).Assembly);
    });

    var app = builder.Build();

    app.ConfigureDevelopmentPipeline();
    app.ConfigureStandardPipeline();
    app.ConfigureEndpoints();

    await app.StartAsync().ConfigureAwait(false);
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
