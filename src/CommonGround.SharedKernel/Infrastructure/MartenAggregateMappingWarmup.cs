using System.Reflection;
using CommonGround.SharedKernel.Domain;
using Marten;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommonGround.SharedKernel.Infrastructure;

/// <summary>
/// Builds Marten's mapping for every aggregate at startup, before any request or message is handled.
/// </summary>
/// <remarks>
/// Aggregates aren't stored, but the first <c>FetchForWriting</c> of an aggregate type builds a document mapping for
/// it anyway, so Marten can write the stream key into the aggregate's ID. Because every aggregate's ID is strongly
/// typed, building that mapping adds the ID type to a list Marten keeps without a lock. Other threads read the list
/// while compiling LINQ queries or resolving other aggregates, so they can fail with a
/// <see cref="NullReferenceException"/> inside Marten. Views' IDs are strings, so once the aggregates' mappings are
/// built here, nothing adds to the list later. This runs after Marten has applied the schema, so the mappings don't
/// get tables.
/// </remarks>
internal sealed partial class MartenAggregateMappingWarmup(IDocumentStore store, ILogger<MartenAggregateMappingWarmup> logger) : IHostedService
{
    private static readonly MethodInfo StorageFor = typeof(MartenAggregateMappingWarmup)
        .GetMethod(nameof(BuildMapping), BindingFlags.NonPublic | BindingFlags.Static)!;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var aggregateTypes = AggregateTypes().ToList();
        foreach (var aggregateType in aggregateTypes)
        {
            StorageFor.MakeGenericMethod(aggregateType).Invoke(null, [store]);
        }

        Logs.BuiltMappings(logger, aggregateTypes.Count);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static void BuildMapping<T>(IDocumentStore store)
        where T : notnull =>
        store.Storage.Database.Providers.StorageFor<T>();

    private static IEnumerable<Type> AggregateTypes() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("CommonGround.", StringComparison.Ordinal) == true)
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false } && IsAggregate(t));

    private static bool IsAggregate(Type type)
    {
        for (var t = type.BaseType; t is not null; t = t.BaseType)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(AggregateRoot<>))
            {
                return true;
            }
        }

        return false;
    }

    private static partial class Logs
    {
        [LoggerMessage(0, LogLevel.Information, "Built Marten's mappings for {AggregateCount} aggregates")]
        public static partial void BuiltMappings(ILogger logger, int aggregateCount);
    }
}
