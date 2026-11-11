using Npgsql;

namespace CommonGround.Management.Gateway.Services;

/// <summary>
/// The gateway's own tables in Postgres: sessions and Data Protection keys.
/// </summary>
public sealed class GatewayDatabase(NpgsqlDataSource dataSource)
{
    /// <summary>The schema that holds the gateway's tables.</summary>
    public const string Schema = "management_gateway";

    /// <summary>Gets the data source for the gateway database.</summary>
    public NpgsqlDataSource DataSource { get; } = dataSource;

    /// <summary>
    /// Creates the schema and tables if they don't exist.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task EnsureCreatedAsync(CancellationToken cancellationToken)
    {
        const string sql = $"""
            CREATE SCHEMA IF NOT EXISTS {Schema};
            CREATE TABLE IF NOT EXISTS {Schema}.sessions (
                session_id text PRIMARY KEY,
                user_id text NOT NULL,
                trust_id text NULL,
                access_token text NULL,
                refresh_token text NULL,
                id_token text NULL,
                created_at timestamptz NOT NULL,
                expires_at timestamptz NOT NULL
            );
            CREATE INDEX IF NOT EXISTS ix_sessions_expires_at ON {Schema}.sessions (expires_at);
            CREATE TABLE IF NOT EXISTS {Schema}.data_protection_keys (
                id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                friendly_name text NULL,
                xml text NOT NULL,
                created_at timestamptz NOT NULL DEFAULT now()
            );
            """;

        using var command = DataSource.CreateCommand(sql);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
