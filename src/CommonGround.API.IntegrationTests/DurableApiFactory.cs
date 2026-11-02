using CommonGround.API.IntegrationTests.Flows;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace CommonGround.API.IntegrationTests;

/// <summary>
/// Wolverine extension that runs durable messaging on a single node, as the API does outside tests,
/// with a handler that fails on cue.
/// </summary>
internal sealed class SoloDurabilityExtension : IWolverineExtension
{
    /// <inheritdoc />
    public void Configure(WolverineOptions options)
    {
        options.Durability.Mode = DurabilityMode.Solo;
        options.Discovery.IncludeType(typeof(FailingApplicationSelectedHandler));
    }
}

/// <summary>
/// Boots the API with durable messaging and Marten's event forwarding running, so a domain event
/// reaches every bounded context's handler as it does in production. Tests drive it through
/// <c>TrackActivity()</c> to wait for the fan-out to finish.
/// </summary>
public class DurableApiFactory : ApiFactory
{
    /// <inheritdoc />
    protected override void ConfigureWolverine(IServiceCollection services) =>
        services.AddWolverineExtension<SoloDurabilityExtension>();
}
