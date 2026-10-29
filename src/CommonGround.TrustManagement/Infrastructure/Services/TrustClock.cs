using System.Collections.Concurrent;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.TrustManagement.Domain.Trusts;
using Marten;

namespace CommonGround.TrustManagement.Infrastructure.Services;

/// <summary>Reads each trust's time zone from its stream once and caches it.</summary>
public sealed class TrustClock(IDocumentStore store) : ITrustClock
{
    private readonly ConcurrentDictionary<string, IanaTimeZone> _timeZones = new(StringComparer.Ordinal);

    /// <inheritdoc/>
    public async Task<IanaTimeZone> GetTimeZoneAsync(string trustId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trustId);

        if (_timeZones.TryGetValue(trustId, out var cached))
        {
            return cached;
        }

        // Trust streams are not tenanted: they are written from the default-tenant session.
        var session = store.QuerySession();
        await using var _ = session.ConfigureAwait(false);
        var trust = await session.Events.AggregateStreamAsync<Trust>(trustId, token: cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Trust '{trustId}' was not found.");

        return _timeZones.GetOrAdd(trustId, trust.TimeZone);
    }
}
