using System.Text.Json;
using CommonGround.Identity.Domain.Users;
using CommonGround.Identity.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>Seeds development users in the identity store and wires them to their trust.</summary>
public sealed class UsersSeeder : IDevelopmentSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <inheritdoc/>
    public async Task SeedAsync(IDocumentStore store, CancellationToken cancellation)
    {
        ArgumentNullException.ThrowIfNull(store);

        var entries = await LoadEntriesAsync(cancellation).ConfigureAwait(false);
        if (entries.Count == 0)
        {
            return;
        }

        // Users are pre-trust global entities — always use the default (non-tenanted) session.
        using var session = store.LightweightSession();

        foreach (var entry in entries)
        {
            var trustLookup = await session
                .LoadAsync<TrustSeedLookup>(TrustSeedLookup.BuildId(entry.TrustSeedKey), cancellation)
                .ConfigureAwait(false)
                ?? throw new InvalidOperationException(
                    $"Trust seed '{entry.TrustSeedKey}' not found for user seed '{entry.SeedKey}'. Ensure TrustsSeeder runs first.");

            var lookupId = UserSeedLookup.BuildId(entry.SeedKey);
            var lookup = await session.LoadAsync<UserSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                var streamState = await session.Events.FetchStreamStateAsync(lookup.UserId, cancellation).ConfigureAwait(false);
                if (streamState is not null)
                {
                    continue;
                }
            }

            var userId = UserId.Generate();
            var registered = User.Register(userId, entry.KeycloakSubject, entry.Email, entry.DisplayName);
            var assigned = User.Create(registered).AssignTrustIfNone(TrustId.Create(trustLookup.TrustId));

            session.Events.StartStream<User>(userId.Value, registered, assigned!);

            session.Store(new UserSeedLookup
            {
                Id = lookupId,
                SeedKey = entry.SeedKey,
                UserId = userId.Value,
                SeededAt = DateTimeOffset.UtcNow,
            });

            await session.SaveChangesAsync(cancellation).ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<UserSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(UsersSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("users.seed.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException("Could not find embedded user seed resource 'users.seed.json'.");
        }

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<UserSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicate = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate is not null)
            {
                throw new InvalidOperationException($"Duplicate user seed key '{duplicate.Key}' in seed JSON.");
            }

            return entries;
        }
    }

    private sealed class UserSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string KeycloakSubject { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? DisplayName { get; set; }

        public string TrustSeedKey { get; set; } = string.Empty;
    }
}
