using CommonGround.Homeowner.Gateway.Configuration;
using Microsoft.Extensions.Options;

namespace CommonGround.Homeowner.Gateway.Services;

/// <summary>
/// Stores each session's tokens as a row in <c>sessions</c>, with an expiry time.
/// Expired rows are never returned; <see cref="GatewayDatabaseMaintenance"/> deletes them.
/// </summary>
public sealed class PostgresTokenStore(
    GatewayDatabase database,
    IOptions<TokenStoreOptions> options,
    TimeProvider timeProvider) : ITokenStore
{
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(options.Value.DefaultTtlMinutes);

    /// <inheritdoc/>
    public async Task SaveTokensAsync(string sessionId, string userId, string? trustId, string? accessToken, string? refreshToken, string? idToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        const string sql = $"""
            INSERT INTO {GatewayDatabase.Schema}.sessions
                (session_id, user_id, trust_id, access_token, refresh_token, id_token, created_at, expires_at)
            VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
            ON CONFLICT (session_id) DO UPDATE SET
                user_id = EXCLUDED.user_id,
                trust_id = EXCLUDED.trust_id,
                access_token = EXCLUDED.access_token,
                refresh_token = EXCLUDED.refresh_token,
                id_token = EXCLUDED.id_token,
                created_at = EXCLUDED.created_at,
                expires_at = EXCLUDED.expires_at
            """;

        var now = timeProvider.GetUtcNow();
        using var command = database.DataSource.CreateCommand(sql);
        command.Parameters.AddWithValue(sessionId);
        command.Parameters.AddWithValue(userId);
        command.Parameters.AddWithValue((object?)trustId ?? DBNull.Value);
        command.Parameters.AddWithValue((object?)accessToken ?? DBNull.Value);
        command.Parameters.AddWithValue((object?)refreshToken ?? DBNull.Value);
        command.Parameters.AddWithValue((object?)idToken ?? DBNull.Value);
        command.Parameters.AddWithValue(now);
        command.Parameters.AddWithValue(now.Add(_ttl));
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<(string userId, string? trustId, string? accessToken, string? refreshToken, string? idToken)?> TryGetTokensAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        const string sql = $"""
            SELECT user_id, trust_id, access_token, refresh_token, id_token
            FROM {GatewayDatabase.Schema}.sessions
            WHERE session_id = $1 AND expires_at > $2
            """;

        using var command = database.DataSource.CreateCommand(sql);
        command.Parameters.AddWithValue(sessionId);
        command.Parameters.AddWithValue(timeProvider.GetUtcNow());
        using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return (
            reader.GetString(0),
            reader.IsDBNull(1) ? null : reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.IsDBNull(4) ? null : reader.GetString(4));
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        using var command = database.DataSource.CreateCommand(
            $"DELETE FROM {GatewayDatabase.Schema}.sessions WHERE session_id = $1");
        command.Parameters.AddWithValue(sessionId);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes every session whose expiry time has passed.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of sessions deleted.</returns>
    public async Task<int> DeleteExpiredAsync(CancellationToken cancellationToken)
    {
        using var command = database.DataSource.CreateCommand(
            $"DELETE FROM {GatewayDatabase.Schema}.sessions WHERE expires_at <= $1");
        command.Parameters.AddWithValue(timeProvider.GetUtcNow());
        return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
